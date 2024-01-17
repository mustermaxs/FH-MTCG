using System;
using System.Data;

namespace MTCG;

public class PackageRepository : BaseRepository<Package>, IRepository<Package>, IService
{
    private static object packageLock = new();
    public PackageRepository()
        : base()
    {

        _Table = "packages";
        _Fields = "*";
    }

    public override IEnumerable<Package> GetAll()
    {
        var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
        List<Package> packages = new List<Package>();
        List<Guid> packageIds = GetAllPackageIds().ToList();
        List<Card> packageCards = new List<Card>();

        foreach (Guid packageId in packageIds)
        {
            List<Guid> packageCardIds = GetPackageCardIds(packageId).ToList();
            var package = new Package();
            package.Id = packageId;
            package.CardIds = packageCardIds;
            List<Card> cards = cardRepo.GetCardsByIds(packageCardIds);
            package.Cards = cards;
            packages.Add(package);
        }

        return packages;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Gets all cards that are contained in a package.
    /// </summary>
    /// <param name="packageCardIds"></param>
    /// <param name="cardRepo"></param>
    /// <returns>
    /// List of cards.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a card is not found.
    /// </exception>
    private List<Card> GetCardsForPackage(List<Guid> packageCardIds)
    {
        var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
        List<Card> cards = new List<Card>();

        foreach (Guid cardId in packageCardIds)
        {
            var card = cardRepo.Get(cardId) ?? throw new Exception($"Failed to get all cards for package.\nCardId {cardId}");
            cards.Add(card);
        }

        return cards;
    }


    /// <summary>
    /// Gets all ids of all available packages.
    /// </summary>
    /// <returns>
    /// IEnumerable of all package ids.
    /// </returns>
    public IEnumerable<Guid> GetAllPackageIds()
    {
        ObjectBuilder<ValueTypeWrapper<Guid>> objectBuilder =
            (ValueTypeWrapper<Guid> v, IDataReader re) => v.Value = re.GetGuid(re.GetOrdinal("id"));

        var builder = new QueryBuilder(this.Connect());
        builder
            .Select("id")
            .From("packages")
            .Build();

        var list = builder.ReadMultiple<ValueTypeWrapper<Guid>>(objectBuilder);

        return list.Select(e => e.Value);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets all card ids for a given package id.
    /// </summary>
    /// <param name="packageId">
    /// The package id.
    /// </param>
    /// <returns>
    /// IEnumerable of all card ids for the given package id.
    /// </returns>
    protected IEnumerable<Guid> GetPackageCardIds(Guid packageId)
    {
        ObjectBuilder<ValueTypeWrapper<Guid>> objectBuilder = (ValueTypeWrapper<Guid> v, IDataReader re) => v.Value = re.GetGuid(re.GetOrdinal("cardid"));
        var builder = new QueryBuilder(this.Connect());
        builder
            .Select("p.id", "pc.cardid as cardid", "pc.packageid")
            .From("packages p")
            .Join("packagecards pc")
            .On("pc.packageid=p.id")
            .Where("p.id=@packageid")
            .AddParam("@packageid", packageId)
            .Build();

        var list = builder.ReadMultiple<ValueTypeWrapper<Guid>>(objectBuilder);

        return list.Select(e => e.Value);
    }

    public override Package? Get(Guid id)
    {
        var packageId = GetAllPackageIds().SingleOrDefault<Guid>(p => p == id);

        if (packageId == Guid.Empty) return null;

        var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
        var package = new Package();
        var packageCardIds = GetPackageCardIds(packageId).ToList();
        var cards = cardRepo.GetCardsByIds(packageCardIds);

        package.Id = packageId;
        package.CardIds = packageCardIds;
        package.Cards = cards;

        return package;
    }

    public override void Save(Package package)
    {
        Guid packageId = AddToPackageTable();

        var pBuilder = new QueryBuilder(this.Connect());
        pBuilder
            .InsertInto("packagecards", "packageid", "cardid");

        int i = 1;

        foreach (Card card in package.Cards)
        {
            pBuilder.InsertValues($"@packageid{i}", $"@cardid{i}")
                .AddParam($"@packageid{i}", packageId)
                .AddParam($"@cardid{i}", card.Id);
            i++;
        }

        pBuilder.Build();
        pBuilder.ExecuteNonQuery();
    }

    protected Guid AddToPackageTable()
    {
        var builder = new QueryBuilder(this.Connect());
        builder
            .InsertInto("packages")
            .InsertValues()
            .GetInsertedIds(true)
            .Build();

        Guid? insertedId = builder.ReadSingle<Guid>("id");

        return insertedId ?? Guid.Empty;
    }

    protected override void Fill(Package obj, IDataReader re)
    {
        obj.Id = re.GetGuid(re.GetOrdinal("id"));
    }
    protected override void _Fill(Package obj, IDataReader re)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes a package from the database along with its
    /// associated cards in the packagecards table.
    /// </summary>
    /// <param name="obj"></param>
    public override void Delete(Package obj)
    {
        lock (packageLock)
        {
            var builder = new QueryBuilder(this.Connect());

            builder
                .DeleteFrom("packages")
                .Where("id=@id")
                .AddParam("@id", obj.Id)
                .Build();

            builder.ExecuteNonQuery();
        }
    }


}
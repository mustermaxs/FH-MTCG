using System;
using System.Data;

namespace MTCG;

public class PackageRepository : BaseRepository<Package>, IRepository<Package>
{
    public PackageRepository()
        : base()
    {

        _Table = "packages";
        _Fields = "*";
    }
    public override IEnumerable<Package> GetAll()
    {
        ObjectBuilder<Package> objectBuilder = Fill;
        var builder = new QueryBuilder(this.Connect());
        builder
            .Select("p.id", "pc.cardid", "pc.packageid")
            .From("packages p")
            .Join("packagecards pc")
            .On("pc.packageid=p.id")
            .Build();

        return builder.ReadMultiple<Package>(objectBuilder);
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
            .GetInsertedIds()
            .Build();

        IEnumerable<Guid> insertedIds = builder.Run<Guid>();

        return insertedIds.FirstOrDefault();
    }

    protected override void Fill(Package obj, IDataReader re)
    {
        throw new NotImplementedException();
    }
    protected override void _Fill(Package obj, IDataReader re)
    {
        throw new NotImplementedException();
    }

    public override void Delete(Package obj)
    {
        throw new NotImplementedException();
    }


}
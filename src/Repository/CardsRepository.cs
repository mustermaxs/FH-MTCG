using System;
using System.Data;
using Npgsql;

namespace MTCG;

public class CardRepository : BaseRepository<Card>, IRepository<Card>
{
    public CardRepository()
        : base()
    {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<CardName>("cardname");

        _Table = "cards";
        _Fields = "id, type, element, name, descr, damage";
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override void Save(Card card)
    {
        using (NpgsqlConnection? connection = this.Connect())
        using (var command = new NpgsqlCommand($"INSERT INTO {_Table} (name, descr, damage, type, element) VALUES (@name, @descr, @damage, @type, @element);", connection))
        {
            command.Parameters.AddWithValue("@name", card.Name);
            command.Parameters.AddWithValue("@description", card.Description);
            command.Parameters.AddWithValue("@type", card.Type.ToString());
            command.Parameters.AddWithValue("@element", card.Element);
            command.Parameters.AddWithValue("@damage", card.Damage);

            command.ExecuteNonQuery();

            command.Dispose(); connection!.Dispose();
        }
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void AddCardToStack(Card card, Guid userId)
    {
        var builder = new QueryBuilder(Connect());

        builder
            .InsertInto("stackcards", "userid", "cardid")
            .InsertValues("@userid", "@cardid")
            .AddParam("@userid", userId)
            .AddParam("@cardid", card.Id)
            .Build();

        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void RemoveCardFromStack(Card card, Guid userId)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom("stack")
            .Where("userid=@userid")
            .AddParam("@userid", userId)
            .And("cardid=@cardid")
            .AddParam("@cardid", card.Id)
            .Limit("1")
            .Build();

        builder.ExecuteNonQuery();
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public void RemoveCardFromDeck(Card card, Guid userId)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom("decks")
            .Where("userid=@userid")
            .AddParam("@userid", userId)
            .And("cardid=@cardid")
            .AddParam("@cardid", card.Id)
            .Limit("1")
            .Build();

        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Obsolete("")]
    public Guid? GetStackIdForUserId(Guid userId)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .Select("id")
            .From("stack")
            .Where("userid=@userid")
            .AddParam("@userid", userId)
            .Build();

        var records = builder.ReadMultiple().ToArray();

        if (records.Count() > 0)
            return (Guid)records[0]["id"];

        return null;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override IEnumerable<Card> GetAll()
    {
        ObjectBuilder<Card> fill = Fill;
        var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From("cards")
            .Build();

        return builder.ReadMultiple<Card>(fill);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public IEnumerable<Card> GetAllCardsInStackByUserId(Guid userid)
    {
        var builder = new QueryBuilder(this.Connect());
        ObjectBuilder<Card> fill = Fill;
        builder
            .Select("c.*")
            .From("cards c")
            .Join("stackcards sc")
            .On("sc.cardid=c.id")
            .Where("sc.userid=@userid")
            .AddParam("@userid", userid)
            .Build();

        var cards = builder.ReadMultiple<Card>(fill);

        return cards;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    // [Obsolete("")]
    public void AddPackage(IEnumerable<Card> cards)
    {
        Guid packageId = Guid.Parse("d199ee35-d0bc-4701-9985-47ec7c8ee180");

        var builder = new QueryBuilder(this.Connect());
        builder
            .InsertInto("packagecards", "packageid", "cardid");

        int i = 1;

        foreach (Card card in cards)
        {
            builder.InsertValues($"@packageid{i}", $"@cardid{i}")
            .AddParam($"@packageid{i}", packageId)
            .AddParam($"@cardid{i}", card.Id);
            i++;
        }

        builder.Build();
        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public IEnumerable<Card>? GetCardsInDeckByUserId(Guid userId)
    {
        ObjectBuilder<Card> objectBuilder = Fill;
        var builder = new QueryBuilder(Connect());
        builder
            .Select("c.*", "d.*")
            .From("cards c")
            .Join("deck d")
            .On("d.cardid=c.id")
            .Where("d.userid=@userid")
            .AddParam("userid", userId)
            .Build();

        IEnumerable<Card>? cards = builder.ReadMultiple<Card>(objectBuilder);

        return cards ?? null;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void AddCardsToDeck(IEnumerable<Card> cards, Guid userId)
    {
        var builder = new QueryBuilder(Connect());
        builder.InsertInto("deck", "cardid", "userid");
        int i = 0;

        foreach (Card card in cards)
        {
            builder
                .InsertValues($"@cardid{i}", $"@userid{i}")
                .AddParam($"@cardid{i}", card.Id)
                .AddParam($"@userid{i}", userId);
        }

        builder.Build();
        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected override void Fill(Card card, IDataReader re)
    {
        card.Id = re.GetGuid(re.GetOrdinal("id"));
        card.Name = (CardName)Enum.Parse(typeof(CardName), re.GetString(re.GetOrdinal("name")));
        card.Description = re.GetString(re.GetOrdinal("descr"));
        card.Damage = (float)re.GetDouble(re.GetOrdinal("damage"));
        card.Element = re.GetString(re.GetOrdinal("element"));
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected override void _Fill(Card card, IDataReader re)
    {
        card.Id = re.GetGuid(re.GetOrdinal("id"));
        card.Name = (CardName)Enum.Parse(typeof(CardName), re.GetString(re.GetOrdinal("name")));
        card.Description = re.GetString(re.GetOrdinal("descr"));
        card.Damage = (float)re.GetDouble(re.GetOrdinal("damage"));
        card.Element = re.GetString(re.GetOrdinal("element"));
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override Card? Get(Guid id)
    {
        ObjectBuilder<Card> fill = Fill;
        var builder = new QueryBuilder(Connect());
        builder
        .Select("*")
        .From("cards")
        .Where("id=@id")
        .AddParam("id", id)
        .Build();

        return builder.Read<Card>(fill);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override void Delete(Card card)
    {
        throw new NotImplementedException();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void Update(Card card)
    {
        throw new NotImplementedException();
    }
}
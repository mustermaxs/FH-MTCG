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

    public void SaveCardToStack(Card card, Guid userId)
    {
        using (NpgsqlConnection? connection = this.Connect())
        using (var command = new NpgsqlCommand($"INSERT INTO stack (cardid, userid) VALUES (@cardid, @userid);", connection))
        {
            command.Parameters.AddWithValue("@cardid", card.Id);
            command.Parameters.AddWithValue("@userid", userId);

            command.ExecuteNonQuery();

            command.Dispose(); connection!.Dispose();
        }
    }

    public override IEnumerable<Card> GetAll()
    {
        // using (NpgsqlConnection? connection = this.Connect())
        // using (var command = new NpgsqlCommand($"SELECT {_Fields} FROM {_Table} ", connection))
        // {
        //     IDataReader re = command.ExecuteReader();

        //     List<Card> cards = new List<Card>();

        //     while (re.Read())
        //     {
        //         Card card = new Card();
        //         _Fill(card, re);
        //         cards.Add(card);
        //     }

        //     command.Dispose(); connection!.Dispose();

        //     return cards;
        // }
        ObjectBuilder<Card> fill = Fill;

        var query = new QueryBuilder(Connect());
        query
        .Select(_Fields.Split(", "))
        .From(_Table).Build();

        return query.ReadMultiple<Card>(fill);
    }

    //TODO
    public void AddCardToStack(Card card, Guid userid)
    {
        // using (NpgsqlConnection? connection = this.Connect())
        // using (var command = new NpgsqlCommand($"INSERT INTO stack (userid, cardid) VALUES(@userid, @cardid)", connection))
        // {
        //     command.Parameters.AddWithValue("@cardid", card.Id);
        //     command.Parameters.AddWithValue("@userid", userid);

        //     command.ExecuteNonQuery();
        //     command.Dispose(); connection!.Dispose();

        //     // return cards;

        // }

        // var query = new QueryBuilder(Connect());
        // query
        // .InsertInto("stack", new string[] { "userid", "cardid" })
        // .AddParam("userid", userid)
        // .AddParam("cardid", card.Id);

        // query.Run();
    }
    public IEnumerable<Card> GetAllCardsInStackByUserId(Guid userid)
    {
        var builder = new QueryBuilder(this.Connect());
        ObjectBuilder<Card> fill = Fill;

        builder
            .Select("s.cardid", "s.userid", "c.id", "c.name", "c.descr", "c.type", "c.element", "c.damage")
            .From("cards c")
            .Join("stack s")
            .On("c.id=s.cardid")
            .Where("s.userid=@userid")
            .AddParam("userid", userid).Build();

        var cards = builder.ReadMultiple<Card>(fill);

        return cards;
    }

    // public void AddCardsToPackage(IEnumerable<Card> cards, Guid packageId)
    // {
    //     var builder = new QueryBuilder(Connect());

    //     foreach (Card card in cards)
    //     {
    //         Guid cardId = card.Id;
    //         builder.InsertInto("packagecards", new string[]{"packageId", "cardid"})
    //         .AddParam("cardId", cardId)
    //         .AddParam()
    //     }
    // }
    // [Obsolete("")]
    public void AddPackage(IEnumerable<Card> cards)
    {
        // Guid packageId = AddToPackageTable();
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

        // return packageId;
    }

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

    protected override void Fill(Card card, IDataReader re)
    {
        card.Id = re.GetGuid(re.GetOrdinal("id"));
        card.Name = (CardName)Enum.Parse(typeof(CardName), re.GetString(re.GetOrdinal("name")));
        card.Description = re.GetString(re.GetOrdinal("descr"));
        card.Damage = (float)re.GetDouble(re.GetOrdinal("damage"));
        card.Element = re.GetString(re.GetOrdinal("element"));
    }
    protected override void _Fill(Card card, IDataReader re)
    {
        card.Id = re.GetGuid(re.GetOrdinal("id"));
        card.Name = (CardName)Enum.Parse(typeof(CardName), re.GetString(re.GetOrdinal("name")));
        card.Description = re.GetString(re.GetOrdinal("descr"));
        card.Damage = (float)re.GetDouble(re.GetOrdinal("damage"));
        card.Element = re.GetString(re.GetOrdinal("element"));
    }

    public override Card? Get(Guid id)
    {
        // return base.Get(id);
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

    public override void Delete(Card card)
    {
        throw new NotImplementedException();
    }

    public void Update(Card card)
    {
        throw new NotImplementedException();
    }
}
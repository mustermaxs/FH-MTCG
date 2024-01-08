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


    /// <summary>
    /// Adds card to cards table in DB.
    /// </summary>
    /// <param name="card"></param>
    public override void Save(Card card)
    {
        // using (NpgsqlConnection? connection = this.Connect())
        // using (var command = new NpgsqlCommand($"INSERT INTO {_Table} (name, descr, damage, type, element) VALUES (@name, @descr, @damage, @type, @element);", connection))
        // {
        //     command.Parameters.AddWithValue("@name", card.Name);
        //     command.Parameters.AddWithValue("@description", card.Description);
        //     command.Parameters.AddWithValue("@type", card.Type.ToString());
        //     command.Parameters.AddWithValue("@element", card.Element);
        //     command.Parameters.AddWithValue("@damage", card.Damage);

        //     command.ExecuteNonQuery();

        //     command.Dispose(); connection!.Dispose();
        // }
        var builder = new QueryBuilder(Connect());
        builder
            .InsertInto("cards", "name", "descr", "damage", "type", "element")
            .InsertValues("@name", "@descr", "@damage", "@type", "@element")
            .AddParam("@name", card.Name.ToString())
            .AddParam("@descr", card.Description)
            .AddParam("@damage", card.Damage)
            .AddParam("@type", card.Type.ToString())
            .AddParam("@element", card.Element)
            .Build();

        builder.ExecuteNonQuery();
    }


    // BUG: Npgsql.PostgresException (0x80004005): 42601: syntax error at or near "DEFAULT"
    public Guid SaveAndGetInsertedId(Card card)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .InsertInto("cards", "name", "descr", "damage", "type", "element")
            .InsertValues("@name", "@descr", "@damage", "@type", "@element")
            .AddParam("@name", card.Name.ToString())
            .AddParam("@descr", card.Description)
            .AddParam("@damage", card.Damage)
            .AddParam("@type", card.Type.ToString())
            .AddParam("@element", card.Element)
            .GetInsertedIds(true)
            .Build();

        // builder.ExecuteNonQuery();
        Guid? insertedId = builder.ReadSingle<Guid>("id");

        return insertedId ?? Guid.Empty;
        // return new Guid();
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


    public Guid? GetStackIdOfCard(Guid cardId, Guid userId)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .Select("id")
            .From("stackcards")
            .Where("userid=@userid")
            .AddParam("@userid", userId)
            .And("cardid=@cardid")
            .AddParam("@cardid", cardId)
            .Limit("1")
            .Build();

        return builder.ReadSingle<Guid>("id");
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void RemoveCardFromStack(Card card, Guid userId)
    {
        Guid? stackCardId = GetStackIdOfCard(card.Id, userId);

        if (!stackCardId.HasValue) throw new Exception("Failed to get stackid of card.");

        var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom("stackcards")
            .Where("id=@id")
            .AddParam("@id", stackCardId)
            .Build();

        builder.ExecuteNonQuery();
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public void RemoveCardFromDeck(Card card, Guid userId)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .Select("id")
            .From("deck")
            .Where("userid=@userid")
            .AddParam("@userid", userId)
            .And("cardid=@cardid")
            .AddParam("@cardid", card.Id)
            .Limit("1")
            .Build();

        Guid deckId = (Guid)builder.ReadMultiple().ToList()[0]["id"];

        builder.Reset();
        var pbuilder = new QueryBuilder(Connect());
        pbuilder
            .DeleteFrom("deck")
            .Where("id=@id")
            .AddParam("@id", deckId)
            .And("userid=@userid")
            .AddParam("@userid", userId)
            .Build();

        pbuilder.ExecuteNonQuery();
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

    /// <summary>
    /// Gets all cards from cards table.
    /// Not specific to any user.
    /// </summary>
    /// <returns>IEnumerable<Cards></returns>
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


    public IEnumerable<StackCard> GetAllCardsInStackByUserId(Guid userid)
    {
        var builder = new QueryBuilder(this.Connect());
        ObjectBuilder<StackCard> fill = Fill;
        builder
            .Select("c.*", "sc.id as stackid")
            .From("cards c")
            .Join("stackcards sc")
            .On("sc.cardid=c.id")
            .Where("sc.userid=@userid")
            .AddParam("@userid", userid)
            .Build();

        var cards = builder.ReadMultiple<StackCard>(fill);

        return cards;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Adds buyable package of cards to packages/packagecards tables.
    /// </summary>
    /// <param name="cards"></param>
    /// TEST



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public IEnumerable<DeckCard>? GetDeckByUserId(Guid userId)
    {
        ObjectBuilder<DeckCard> objectBuilder = Fill;
        var builder = new QueryBuilder(Connect());
        builder
            .Select("c.*", "d.cardid", "d.userid", "d.id as deckid", "d.locked as locked")
            .From("cards c")
            .Join("deck d")
            .On("d.cardid=c.id")
            .Where("userid=@userid")
            .AddParam("userid", userId)
            .Build();

        List<DeckCard>? cards = builder.ReadMultiple<DeckCard>(objectBuilder).ToList();

        return cards ?? null;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public DeckCard? GetDeckCardForUser(Guid cardId, Guid userId)
    {
        ObjectBuilder<DeckCard> objectBuilder = Fill;
        var builder = new QueryBuilder(Connect());
        builder
            .Select("c.*", "d.*", "d.id as deckid")
            .From("cards c")
            .Join("deck d")
            .On("d.cardid=c.id")
            .Where("d.userid=@userid")
            .AddParam("@userid", userId)
            .And("d.cardid=@cardid")
            .AddParam("@cardid", cardId)
            .Build();

        DeckCard? card = builder.Read<DeckCard>(objectBuilder);

        return card ?? null;
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
            i++;
        }

        builder.Build();
        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////





    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected override void Fill(Card card, IDataReader re)
    {
        if (card is StackCard)
            FillStackCard(card as StackCard, re);
        if (card is DeckCard)
            FillDeckCard(card as DeckCard, re);

        card.Id = re.GetGuid(re.GetOrdinal("id"));
        card.Name = (CardName)Enum.Parse(typeof(CardName), re.GetString(re.GetOrdinal("name")));
        card.Description = re.GetString(re.GetOrdinal("descr"));
        card.Damage = (float)re.GetDouble(re.GetOrdinal("damage"));
        card.Element = re.GetString(re.GetOrdinal("element"));
        card.Type = re.GetString(re.GetOrdinal("type"));
    }


    protected void FillStackCard(StackCard card, IDataReader re)
    {
        card.StackId = re.GetGuid(re.GetOrdinal("stackid"));
    }

    protected void FillDeckCard(DeckCard card, IDataReader re)
    {
        card.DeckId = re.GetGuid(re.GetOrdinal("deckid"));
        card.Locked = re.GetBoolean(re.GetOrdinal("locked"));
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
        var builder = new QueryBuilder(Connect());

        builder
            .DeleteFrom("cards")
            .Where("id=@cardid")
            .AddParam("@cardid", card.Id)
            .Build();
        
        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    public void DeleteAll()
    {
        var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom("cards")
            .Build();
        
        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void UpdateDeckCard(DeckCard card)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .Update("deck")
            .UpdateSet("cardid", "@cardid")
            .AddParam("@cardid", card.Id)
            .UpdateSet("locked", "@locked")
            .AddParam("@locked", card.Locked)
            .Where("id=@id")
            .AddParam("@id", card.DeckId!)
            .Build();

        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void Update(Card card)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .Update("cards")
            .UpdateSet("name", "@name")
            .AddParam("@name", card.Name)
            .UpdateSet("descr", "@descr")
            .AddParam("@descr", card.Description!)
            .UpdateSet("damage", "@damage")
            .AddParam("@damage", card.Damage)
            .UpdateSet("element", "@element")
            .AddParam("@element", card.Element!)
            .UpdateSet("type", "@type")
            .AddParam("@type", card.Type!)
            .Where("id=@id")
            .AddParam("@id", card.Id)
            .Build();

        builder.ExecuteNonQuery();
    }

    public List<Card> GetCardsByIds(List<Guid> packageCardIds)
    {
        List<Card> cards = new List<Card>();

        foreach (Guid cardId in packageCardIds)
        {
            var card = Get(cardId) ?? throw new Exception($"Failed to get all cards.\nCardId {cardId}");
            cards.Add(card);
        }

        return cards;
    }
}
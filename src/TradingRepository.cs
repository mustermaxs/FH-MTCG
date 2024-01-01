using System;
using System.Data;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


// TODO
public class TradingRepository : BaseRepository<StoreTrade>, IRepository<StoreTrade>
{
    public TradingRepository()
: base()
    {
        _Table = "trades";
        _Fields = "*";
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public UserTrade? GetTradeById(Guid id)
    {
        var builder = new QueryBuilder(Connect());
        ObjectBuilder<UserTrade> objectBuilder = Fill;
        builder
            .Select("t.*", "c.*")
            .From("trades t")
            .Join("cards c")
            .On("c.id=t.cardid")
            .Where("t.id=@id")
            .AddParam("@id", id)
            .Build();

        UserTrade? trade = builder.Read<UserTrade>(objectBuilder);

        return trade ?? null;
    }

    protected void FillShopTrade(StoreTrade trade, IDataReader re)
    {
        var cardRepo = new CardRepository();
        Guid offeredCardId = re.GetGuid(re.GetOrdinal("offeredcardid"));
        var offeredCard = cardRepo.Get(offeredCardId);

        if (offeredCard == null) throw new Exception("Card doesn't exist.");

        trade.CardToTrade = offeredCard.Id;
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.Type = re.GetString(re.GetOrdinal("requiredtype"));
        trade.MinimumDamage = (float)re.GetDouble(re.GetOrdinal("minimumdamage"));
        trade.SetOfferingUserId(re.GetGuid(re.GetOrdinal("offeringuserid")));
    }


    protected override void Fill(StoreTrade trade, IDataReader re)
    {
        if (trade is StoreTrade)
            FillShopTrade(trade, re);
        else if (trade is UserTrade)
            FillUserTrade(trade as UserTrade, re);
    }


    protected void FillUserTrade(UserTrade trade, IDataReader re)
    {
        var cardRepo = new CardRepository();
        Guid cardId = re.GetGuid(re.GetOrdinal("cardid"));
        var card = cardRepo.Get(cardId);

        if (card == null) throw new Exception("Card doesn't exist.");

        trade.CardToTrade = card.Id;
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.Type = re.GetString(re.GetOrdinal("required"));
        trade.MinimumDamage = (float)re.GetDouble(re.GetOrdinal("minimumdamage"));
        trade.SetOfferingUserId(re.GetGuid(re.GetOrdinal("offeringuserid")));
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    protected override void _Fill(StoreTrade trade, IDataReader re)
    {
        var cardRepo = new CardRepository();
        Guid cardId = re.GetGuid(re.GetOrdinal("cardid"));
        var card = cardRepo.Get(cardId);

        if (card == null) throw new Exception("Card doesn't exist.");

        trade.CardToTrade = card.Id;
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.Type = re.GetString(re.GetOrdinal("requiredtype"));
        trade.MinimumDamage = (float)re.GetDouble(re.GetOrdinal("minimumdamage"));
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public IEnumerable<T> GetAll<T>() where T : StoreTrade, new()
    {
        ObjectBuilder<T> objectBuilder = Fill;

        var builder = new QueryBuilder(Connect());
        Type tradeType = typeof(T);
        string tradeTypeStringVal =
            tradeType == typeof(UserTrade)
            ? "users"
            : "store";

        builder
            .Select("*")
            .From("trades")
            .Where("tradetype=@tradetype")
            .AddParam("@tradetype", tradeTypeStringVal)
            .Build();

        IEnumerable<T> trades = builder.ReadMultiple<T>(objectBuilder);

        return trades;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public IEnumerable<UserTrade> GetAllStoreTrades()
    {
        ObjectBuilder<UserTrade> objectBuilder = Fill;

        var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From("trades")
            .Where("tradetype=@tradetype")
            .AddParam("@tradetype", "store")
            .Build();

        IEnumerable<UserTrade> trades = builder.ReadMultiple<UserTrade>(objectBuilder);

        return trades;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public IEnumerable<UserTrade> GetAllPendingUserTrades()
    {
        ObjectBuilder<UserTrade> objectBuilder = Fill;

        var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From("trades")
            .Where("tradetype=@tradetype")
            .AddParam("@tradetype", "users")
            .Build();

        IEnumerable<UserTrade> trades = builder.ReadMultiple<UserTrade>(objectBuilder);

        return trades;
    }

    public override StoreTrade? Get(Guid id)
    {
        return base.Get(id);
    }

    public override void Save(StoreTrade obj)
    {
        if (obj is UserTrade userTrade)
        {
            if (userTrade != null)
            {
                SaveTradeAmongUsers(userTrade);
            }
        }
        else
        {
            SaveTradeWithStore(obj);
        }
    }

    protected void SaveTradeWithStore(StoreTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .InsertInto("trades", "cardid", "minimumdamage", "type", "tradetype")
            .InsertValues("@cardid", "@minimumdamage", "@type", "@tradetype")
            .AddParam("@cardid", obj.CardToTrade!)
            .AddParam("@minimumdamage", obj.MinimumDamage)
            .AddParam("@type", obj.Type!)
            .AddParam("@tradetype", "store")
            .Build();

        builder.ExecuteNonQuery();
    }
    protected void SaveTradeAmongUsers(UserTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .InsertInto("trades", "offeringuserid", "offeredcardid", "minimumdamage", "requiredtype", "tradetype")
            .InsertValues("@offeringuserid", "@offeredcardid", "@minimumdamage", "@requiredtype", "@tradetype")
            .AddParam("@offeringuserid", obj.GetOfferingUserId())
            .AddParam("@offeredcardid", obj.CardToTrade)
            .AddParam("@minimumdamage", obj.MinimumDamage)
            .AddParam("@requiredtype", obj.Type)
            .AddParam("@tradetype", "users")
            .Build();

        builder.ExecuteNonQuery();
    }

    public override void Delete(StoreTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom("trades")
            .Where("id=@id")
            .AddParam("@id", obj.Id!.Value)
            .Build();
        builder.ExecuteNonQuery();
    }
}
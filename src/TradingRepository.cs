using System;
using System.Data;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


// TODO
public class TradingRepository : BaseRepository<Trade>, IRepository<Trade>
{
    public TradingRepository()
: base()
    {
        _Table = "trades";
        _Fields = "*";
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    public Trade? GetTradeById(Guid id)
    {
        var builder = new QueryBuilder(Connect());
        ObjectBuilder<Trade> objectBuilder = Fill;
        builder
            .Select("t.*", "c.*")
            .From("trades t")
            .Join("cards c")
            .On("c.id=t.cardid")
            .Where("t.id=@id")
            .AddParam("@id", id)
            .Build();
        
        Trade? trade = builder.Read<Trade>(objectBuilder);

        return trade ?? null;
    }


    protected override void Fill(Trade trade, IDataReader re)
    {
        var cardRepo = new CardRepository();
        Guid cardId = re.GetGuid(re.GetOrdinal("cardid"));
        var card = cardRepo.Get(cardId);

        if (card == null) throw new Exception("Card doesn't exist.");

        trade.CardToTrade = card;
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.Type = re.GetString(re.GetOrdinal("type"));
        trade.MinimumDamage = re.GetFloat(re.GetOrdinal("minimumdamage"));
    }

    protected override void _Fill(Trade trade, IDataReader re)
    {
        var cardRepo = new CardRepository();
        Guid cardId = re.GetGuid(re.GetOrdinal("cardid"));
        var card = cardRepo.Get(cardId);

        if (card == null) throw new Exception("Card doesn't exist.");

        trade.CardToTrade = card;
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.Type = re.GetString(re.GetOrdinal("type"));
        trade.MinimumDamage = re.GetFloat(re.GetOrdinal("minimumdamage"));
    }

    public IEnumerable<Trade> GetAllOpenTrades()
    {
        ObjectBuilder<Trade> objectBuilder = Fill;

        var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From("trades")
            .Build();
        
        IEnumerable<Trade> trades = builder.ReadMultiple<Trade>(objectBuilder);

        return trades;
    }

    public override Trade? Get(Guid id)
    {
        return base.Get(id);
    }

    public override void Save(Trade obj)
    {
        throw new NotImplementedException();
    }

    public override void Delete(Trade obj)
    {
        base.Delete(obj);
    }
}
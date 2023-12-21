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
        _Table = "tradings";
        _Fields = "*";
    }

    protected override void Fill(Trade trade, IDataReader re)
    {
        // IMPORTANT wie card fillen?
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.CardToTrade = re.GetGuid(re.GetOrdinal("cardid"));
        trade.Type = re.GetString(re.GetOrdinal("type"));
        trade.MinimumDamage = re.GetFloat(re.GetOrdinal("minimumdamage"));
    }

    protected override void _Fill(Trade trade, IDataReader re)
    {
        // IMPORTANT wie card fillen?
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.CardToTrade = re.GetGuid(re.GetOrdinal("cardid"));
        trade.Type = re.GetString(re.GetOrdinal("type"));
        trade.MinimumDamage = (float)re.GetDouble(re.GetOrdinal("minimumdamage"));
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
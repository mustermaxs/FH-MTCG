using System;
using System.Data;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


// TODO
public class CardTradeRepository : BaseRepository<CardTrade>, IRepository<CardTrade>
{
    public CardTradeRepository()
: base()
    {
        _Table = "cardtrades";
        _Fields = "*";
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    protected override void Fill(CardTrade trade, IDataReader re)
    {
        var cardRepo = new CardRepository();
        Guid offeredCardId = re.GetGuid(re.GetOrdinal("offeredcardid"));
        var offeredCard = cardRepo.Get(offeredCardId);

        if (offeredCard == null) throw new Exception("Card doesn't exist.");

        trade.CardToTrade = offeredCard.Id;
        trade.Id = re.GetGuid(re.GetOrdinal("id"));
        trade.Type = re.GetString(re.GetOrdinal("requiredtype"));
        trade.MinimumDamage = (float)re.GetDouble(re.GetOrdinal("minimumdamage"));
        trade.OfferingUserId = (re.GetGuid(re.GetOrdinal("offeringuserid")));
        trade.AcceptingUserId = re.IsDBNull(re.GetOrdinal("acceptinguserid")) ? Guid.Empty : re.GetGuid(re.GetOrdinal("acceptinguserid"));
        trade.Settled = re.GetBoolean(re.GetOrdinal("settled"));
    }
    override protected void _Fill(CardTrade trade, IDataReader re)
    {
        Fill(trade, re);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override CardTrade? Get(Guid id)
    {
        return base.Get(id);
    }

    public override void Save(CardTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .InsertInto("cardtrades", "offeringuserid", "offeredcardid", "minimumdamage", "requiredtype")
            .InsertValues("@offeringuserid", "@offeredcardid", "@minimumdamage", "@requiredtype")
            .AddParam("@offeringuserid", obj.OfferingUserId)
            .AddParam("@offeredcardid", obj.CardToTrade)
            .AddParam("@minimumdamage", obj.MinimumDamage)
            .AddParam("@requiredtype", obj.Type)
            .Build();

        builder.ExecuteNonQuery();
    }

    public void Update(CardTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .Update("cardtrades")
            .UpdateSet("offeringuserid", "@ouserid")
            .UpdateSet("offeredcardid", "@ocardid")
            .UpdateSet("minimumdamage", "@min")
            .UpdateSet("requiredtype", "@type")
            .UpdateSet("settled", "@settled")
            .Where("id=@id")
                .AddParam("@id", obj.Id.Value)
                .AddParam("@ouserid", obj.OfferingUserId)
                .AddParam("@ocardid", obj.CardToTrade.Value)
                .AddParam("@min", obj.MinimumDamage)
                .AddParam("@type", obj.Type)
                .AddParam("@settled", obj.Settled)

            .Build();

        builder.ExecuteNonQuery();
    }


    protected void SaveTradeAmongUsers(CardTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .InsertInto("cardtrades", "offeringuserid", "offeredcardid", "minimumdamage", "requiredtype", "tradetype")
            .InsertValues("@offeringuserid", "@offeredcardid", "@minimumdamage", "@requiredtype", "@tradetype")
            .AddParam("@offeringuserid", obj.OfferingUserId)
            .AddParam("@offeredcardid", obj.CardToTrade)
            .AddParam("@minimumdamage", obj.MinimumDamage)
            .AddParam("@requiredtype", obj.Type)
            .AddParam("@tradetype", "users")
            .Build();

        builder.ExecuteNonQuery();
    }

    public override void Delete(CardTrade obj)
    {
        var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom("cardtrades")
            .Where("id=@id")
            .AddParam("@id", obj.Id!.Value)
            .Build();
        builder.ExecuteNonQuery();
    }
}
using System;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

[Controller]
public class TradingController : IController
{
    protected static TradingRepository repo = new TradingRepository();
    public TradingController(IRequest request) : base(request) { }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    // CHANGE -> Role.USER | ROLE.ADMIN
    [Route("/tradings", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAllTradingDeals()
    {
        try
        {
            IEnumerable<Trade> trades = repo.GetAllOpenTrades();

            if (trades.Count() == 0) return new Response<string>(204, "The request was fine, but there are no trading deals available");

            return new Response<IEnumerable<Trade>>(200, trades, "There are trading deals available, the response contains these");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch all pending trading deals.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    // [Route("/tradings/{tradingdealid:int}", HTTPMethod.GET, Role.USER | Role.ADMIN)]
    // public IResponse AcceptTrade(Guid tradingdealid)
    // {
    //     try
    //     {
    //         Trade? trade = repo.GetTradeById(tradingdealid);

    //         if (trade == null) return new Response<string>(404, "The provided deal ID was not found.");


    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Failed to accept trade.\n{ex}");

    //         return new Response<string>(500, "Something went wrong :(");
    //     }
    // }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected bool AcceptedDealMeetsRequirements(Trade trade)
    {
        if (!ValidateAcceptedTrade(trade)) throw new Exception("Accepted trade incomplete.");

        Card acceptedCard = trade.GetAcceptedCard()!;

        bool typeIsSame = trade.Type == acceptedCard.Type;
        bool damageGreaterOrEqual = acceptedCard.Damage >= trade.CardToTrade!.Damage;

        return typeIsSame && damageGreaterOrEqual;
    }


    protected bool ValidateAcceptedTrade(Trade trade)
    {
        return (
            trade.GetAcceptedCard() != null &&
            trade.GetOfferingUser() != null &&
            trade.GetAcceptedCard() != null
            );
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/tradings", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse AddTradingDeal()
    {
        throw new NotImplementedException("Not implemented.");

        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to add new trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/tradings/{tradingdealid:int}", HTTPMethod.DELETE, Role.USER | Role.ADMIN)]
    public IResponse DeleteTradingDeal(int tradingdealid)
    {
        throw new NotImplementedException("Not implemented.");

        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to remove trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/tradings/{tradingdealid:int}", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse HandleAcceptTradingDeal(int tradingdealid)
    {
        throw new NotImplementedException("Not implemented.");

        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to accept trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }
}
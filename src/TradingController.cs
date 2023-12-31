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
        bool damageGreaterOrEqual = acceptedCard.Damage >= trade.MinimumDamage;

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
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            Trade trade = request.PayloadAsObject<Trade>();

            if (trade == null) throw new Exception("");
            
            var cardRepo = new CardRepository();
            List<Card> deckCards = cardRepo.GetAllCardsInStackByUserId(userId).ToList();
            
            if (!deckCards.Exists(c => c.Id == trade.CardToTrade))
                return new Response<string>(403, "The deal contains a card that is not owned by the user or locked in the deck.");
            
            trade.SetOfferingUserId(userId);
            
            repo.Save(trade);

            return new Response<string>(201, "Trading deal successfully created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add new trading deal.\n{ex}");

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
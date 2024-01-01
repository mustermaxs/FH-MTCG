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
    [Route("/tradings?filter={type:alpha}", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAllStoreTradingDeals(string filter)
    {
        try
        {
            var trades = repo.GetAll<StoreTrade>().ToList<StoreTrade>();

            if (trades.Count() == 0) return new Response<string>(204, "The request was fine, but there are no trading deals available");

            return new Response<List<StoreTrade>>(200, trades, "There are trading deals available, the response contains these");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch all pending trading deals.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    
    public IResponse GetAllUserTradingDeals()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            IEnumerable<UserTrade> trades = repo.GetAll<UserTrade>();

            if (trades.Count() == 0) return new Response<string>(204, "The request was fine, but there are no trading deals available");

            return new Response<IEnumerable<UserTrade>>(200, trades, "There are trading deals available, the response contains these");
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


    protected bool AcceptedDealMeetsRequirements(UserTrade trade)
    {
        if (!ValidateAcceptedTrade(trade)) throw new Exception("Accepted trade incomplete.");

        Card acceptedCard = trade.GetAcceptedCard()!;

        bool typeIsSame = trade.Type == acceptedCard.Type;
        bool damageGreaterOrEqual = acceptedCard.Damage >= trade.MinimumDamage;

        return typeIsSame && damageGreaterOrEqual;
    }


    protected bool ValidateAcceptedTrade(UserTrade trade)
    {
        return (
            trade.GetAcceptedCard() != null &&
            trade.GetOfferingUser() != null &&
            trade.GetAcceptedCard() != null
            );
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Adds a new trading deal among users to the DB.
    /// </summary>
    /// <returns></returns>
    [Route("/tradings/users", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse AddTradingDealWithUser()
    {
        try
        {
            UserTrade trade = request.PayloadAsObject<UserTrade>();

            if (trade == null) throw new Exception("");

            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
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


    public IResponse AddTradingDealWithStore()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            UserTrade trade = request.PayloadAsObject<UserTrade>();

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


    [Route("/tradings/{tradingdealid:alphanum}", HTTPMethod.DELETE, Role.USER | Role.ADMIN)]
    public IResponse DeleteTradingDeal(Guid tradingdealid)
    {
        try
        {
            var trade = repo.Get(tradingdealid);

            if (trade == null) return new Response<string>(404, "The provided deal ID was not found.");

            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;

            if (!OfferedCardIsOwnedByUser(trade.CardToTrade!.Value, userId))
                return new Response<string>(403, "The deal contains a card that is not owned by the user.");

            if (trade.GetOfferingUserId() == userId)
                return new Response<string>(404, "The provided deal ID was not found.");

            return new Response<string>(200, "Trading deal successfully deleted");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected bool TradeIsOwnedByUser(Guid tradeId, Guid userId)
    {
        return repo.Get(tradeId).GetOfferingUserId() == userId;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected bool OfferedCardIsOwnedByUser(Guid cardId, Guid userId)
    {
        var cardRepo = new CardRepository();
        var deckCards = cardRepo.GetCardsInDeckByUserId(userId);

        if (deckCards == null || deckCards.Count() == 0)
            return false;

        return deckCards.Any<Card>(c => c.Id == cardId);
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
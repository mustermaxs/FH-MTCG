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
    public IResponse GetAllStoreTradingDeals()
    {
        try
        {
            List<StoreTrade> trades;
            string? queryFilter;
            request.Endpoint.UrlParams.QueryString.TryGetValue("filter", out queryFilter);

            if (queryFilter != null)
            {
                if (queryFilter == "store") trades = repo.GetAll<StoreTrade>().ToList<StoreTrade>();
                else if (queryFilter == "users") trades = repo.GetAll<UserTrade>().ToList<StoreTrade>();
                else throw new Exception("Invalid filter.");
            }
            else trades = repo.GetAll<StoreTrade>().ToList<StoreTrade>();

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
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId!)!.ID;
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
            UserTrade? trade = request.PayloadAsObject<UserTrade>();

            if (trade == null) throw new Exception("");

            Guid userId = SessionManager.GetUserBySessionId(request.SessionId!)!.ID;

            if (!OfferedCardIsOwnedByUser(trade.CardToTrade, userId))
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


    protected bool TradeIsOwnedByUser(StoreTrade trade, Guid userId)
    {
        return trade.GetOfferingUserId() == userId;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected bool OfferedCardIsOwnedByUser(Guid? cardId, Guid? userId)
    {
        if (!cardId.HasValue || !userId.HasValue) return false;

        var cardRepo = new CardRepository();
        var deckCards = cardRepo.GetCardsInDeckByUserId(userId.Value);

        if (deckCards == null || deckCards.Count() == 0)
            return false;

        return deckCards.Any<Card>(c => c.Id == cardId);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    // [Route("/tradings/{tradingdealid:alphanum}", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    // public IResponse AcceptTradingDeal(Guid tradingdealid)
    // {
    //     try
    //     {
    //         var reqParams = request.Endpoint!.UrlParams;
    //         string? queryFilter = "store"; // default value
    //         StoreTrade? trade = null;

    //         if (!reqParams.QueryString.TryGetValue("filter", out queryFilter) || queryFilter == "store")
    //             return HandleAcceptStoreTradingDeal(tradingdealid);
    //         else if (queryFilter == "users")
    //             return HandleAcceptUserTradingDeal(tradingdealid);


    //         if (trade == null)
    //             return new Response<string>(404, "The provided deal ID was not found.");


    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine("Failed to accept trading deal.\n{ex}");

    //         return new Response<string>(500, "Something went wrong :(");
    //     }
    // }

    private IResponse HandleAcceptUserTradingDeal(Guid tradingdealid)
    {
        throw new NotImplementedException();
    }

    // public IResponse HandleAcceptStoreTradingDeal(Guid tradingdealid)
    // {
    //     try
    //     {
    //         StoreTrade? trade = repo.GetTradeById<StoreTrade>(tradingdealid);
    //         bool tradeIsValid = true;

    //         if (trade == null)
    //             return new Response<string>(404, "The provided deal ID was not found.");

    //         Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
    //         var cardRepo = new CardRepository();
    //         List<Card> deckCards = cardRepo.GetAllCardsInStackByUserId(userId).ToList();

    //         if (!OfferedCardIsOwnedByUser(trade.CardToTrade, userId))
    //             tradeIsValid = false;

    //         var cards = cardRepo.GetAll();
    //         var acceptedCard = cards.SingleOrDefault<Card>(c =>
    //             c.Damage >= trade.MinimumDamage &&
    //             c.Type == trade.Type);
            
    //         if (acceptedCard == null)
    //             return new Response<string>(403, "The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
            
    //         cardRepo.AddCardToStack

    //         repo.Save(trade);

    //         return new Response<string>(201, "Trading deal successfully created");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Failed to add new trading deal.\n{ex}");

    //         return new Response<string>(500, "Something went wrong :(");
    //     }
    // }
}
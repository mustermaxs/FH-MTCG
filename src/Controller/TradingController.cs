using System;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

[Controller]
public class CardTradingController : IController
{
    protected static CardTradeRepository repo = new CardTradeRepository();
    public CardTradingController(IRequest request) : base(request) { }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    // CHANGE -> Role.USER | ROLE.ADMIN
    [Route("/tradings", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAllPendingCardTrades()
    {
        try
        {
            List<CardTrade> trades = repo.GetAll().Where(t => !t.Settled).ToList();

            if (trades.Count == 0) return new Response<string>(204, "The request was fine, but there are no trading deals available");

            return new Response<List<CardTrade>>(200, trades, "There are trading deals available, the response contains these");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch all pending trading deals.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/tradings/{tradeid:alphanum}", HTTPMethod.POST, Role.USER)]
    public IResponse AcceptTradingDeal(Guid tradeid)
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId!)!.ID;
            var cardRepo = new CardRepository();
            var reqContent = request.PayloadAsObject<object>();
            Guid acceptedCardId = Guid.Parse(reqContent.ToString());
            Card acceptedCard = cardRepo.GetDeckCardForUser(acceptedCardId, userId);
            CardTrade? trade = repo.Get(tradeid);

            if (trade == null || trade.Settled)
                return new Response<string>(404, "The provided deal ID was not found.");

            Card offeredCard = cardRepo.GetDeckCardForUser(trade.CardToTrade.Value, trade.OfferingUserId);

            if (!OfferedCardIsOwnedByUser(acceptedCardId, userId) ||
                userId == trade.OfferingUserId ||
                !AcceptedDealMeetsRequirements(trade, acceptedCard))
                return new Response<string>(403, "The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");

            trade.AcceptingUserId = userId;
            trade.Settled = true;
            trade.AcceptedDeckCardId = acceptedCardId;
            acceptedCard.Locked = false;
            offeredCard!.Locked = false;
            ExchangeDeckCards(acceptedCard, offeredCard!, userId, trade.OfferingUserId);
            repo.Update(trade);

            return new Response<string>(200, "Trading deal successfully executed.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }




    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected void ExchangeDeckCards(Card acceptedCard, Card offeredCard, Guid userId, Guid offeringUserId)
    {
        var cardRepo = new CardRepository();
        cardRepo.AddCardsToDeck(new List<Card> { acceptedCard }, offeringUserId);
        cardRepo.RemoveCardFromDeck(acceptedCard, userId);
        cardRepo.AddCardsToDeck(new List<Card> { offeredCard }, userId);
        cardRepo.RemoveCardFromDeck(offeredCard, offeringUserId);
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected bool AcceptedDealMeetsRequirements(CardTrade trade, Card acceptedCard)
    {
        var userId = SessionManager.GetUserBySessionId(request.SessionId!)!.ID;
        bool tradeWithSelf = trade.OfferingUserId == userId;
        bool typeIsSame = trade.Type == acceptedCard.Type;
        bool damageGreaterOrEqual = acceptedCard.Damage >= trade.MinimumDamage;

        return typeIsSame && damageGreaterOrEqual && !tradeWithSelf && !trade.Settled;
    }


    // protected bool ValidateAcceptedTrade(CardTrade trade)
    // {
    //     return (
    //         trade.GetAcceptedCard() != null &&
    //         trade.GetOfferingUser() != null &&
    //         trade.GetAcceptedCard() != null
    //         );
    // }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Adds a new trading deal among users to the DB.
    /// </summary>
    /// <returns></returns>
    // [Route("/tradings/users", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse AddTradingDealWithUser()
    {
        try
        {
            var trade = request.PayloadAsObject<CardTrade>();

            if (trade == null) throw new Exception("");

            Guid userId = SessionManager.GetUserBySessionId(request.SessionId!)!.ID;

            if (!OfferedCardIsOwnedByUser(trade.CardToTrade, userId))
                return new Response<string>(403, "The deal contains a card that is not owned by the user or locked in the deck.");

            trade.OfferingUserId = userId;

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

    [Route("/tradings", HTTPMethod.POST, Role.USER)]
    public IResponse AddCardTradingDeal()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId!)!.ID;
            var trade = request.PayloadAsObject<CardTrade>();

            if (trade == null) throw new Exception($"Failed to create trade object from provided JSON string.\n{request.Payload}");

            var cardRepo = new CardRepository();
            DeckCard deckCard = cardRepo.GetDeckCardForUser(trade.CardToTrade.Value, UserId);

            if (deckCard == null || !deckCard.DeckId.HasValue || deckCard.Locked)
                return new Response<string>(403, "The deal contains a card that is not owned by the user or locked in the deck.");

            trade.OfferingUserId = UserId;
            trade.DeckId = deckCard.DeckId;
            deckCard.Locked = true;

            cardRepo.UpdateDeckCard(deckCard);

            repo.Save(trade);

            return new Response<string>(201, "Trading deal successfully created");
        }
        catch (Exception ex)
        {
            string postTradeJsonStructure = @"{
            ""CardToTrade"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Type"": ""monster"",
            ""MinimumDamage"": 15
        }";

            Console.WriteLine($"{ex}\nFailed to add new trading deal.");

            return new Response<string>(500, @$"Something went wrong :(\nPerhaps misconfigured JSON request object.\nReq. structure: {postTradeJsonStructure}");
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

            if (trade == null || trade.Settled || trade.OfferingUserId != UserId)
                return new Response<string>(404, "The provided deal ID was not found.");

            if (!OfferedCardIsOwnedByUser(trade.CardToTrade!.Value, UserId))
                return new Response<string>(403, "The deal contains a card that is not owned by the user.");

            repo.Delete(trade);

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


    protected bool TradeIsOwnedByUser(CardTrade trade, Guid userId)
    {
        return trade.OfferingUserId == userId;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected bool OfferedCardIsOwnedByUser(Guid? cardId, Guid? userId)
    {
        if (!cardId.HasValue || !userId.HasValue) return false;

        var cardRepo = new CardRepository();
        var deckCards = cardRepo.GetDeckByUserId(userId.Value);

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
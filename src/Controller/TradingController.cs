using System;
using System.ComponentModel;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

[Controller]
public class CardTradingController : BaseController
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

            if (trades.Count == 0) return new Response<string>(204, resConfig["TRADE_GETALL_EMPTY"]);

            return new Response<List<CardTrade>>(200, trades, resConfig["TRADE_GETALL_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch all pending trading deals.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
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
            var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
            string reqContent = request.PayloadAsObject<string>();
            Guid acceptedCardId = Guid.Parse(reqContent!);
            DeckCard acceptedCard = cardRepo.GetDeckCardForUser(acceptedCardId, userId);
            CardTrade? trade = repo.Get(tradeid);

            if (trade == null || trade.Settled)
                return new Response<string>(404, resConfig["TRADE_ID_NOT_FOUND"]);

            DeckCard offeredCard = cardRepo.GetDeckCardForUser(trade.CardToTrade.Value, trade.OfferingUserId);

            if (!OfferedCardIsOwnedByUser(acceptedCardId, userId) ||
                userId == trade.OfferingUserId ||
                !AcceptedDealMeetsRequirements(trade, acceptedCard))
                return new Response<string>(403, resConfig["TRADE_ACCEPT_ERR"]);

            trade.AcceptingUserId = userId;
            trade.Settled = true;
            trade.AcceptedDeckCardId = acceptedCardId;
            acceptedCard.Locked = false;
            offeredCard!.Locked = false;
            ExchangeDeckCards(acceptedCard, offeredCard!, userId, trade.OfferingUserId);
            repo.Update(trade);

            return new Response<string>(200, resConfig["TRADE_ACCEPT_SUCC"]);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove trading deal.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }




    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected void ExchangeDeckCards(DeckCard acceptedCard, DeckCard offeredCard, Guid userId, Guid offeringUserId)
    {
        var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
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

            var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
            DeckCard deckCard = cardRepo.GetDeckCardForUser(trade.CardToTrade.Value, UserId);

            if (deckCard == null || !deckCard.DeckId.HasValue || deckCard.Locked)
                return new Response<string>(403, resConfig["TRADE_ADD_ERR"]);

            trade.OfferingUserId = UserId;
            trade.DeckId = deckCard.DeckId;
            deckCard.Locked = true;

            cardRepo.UpdateDeckCard(deckCard);

            repo.Save(trade);

            return new Response<string>(201, resConfig["TRADE_ADD_SUCC"]);
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
                return new Response<string>(404, resConfig["TRADE_DEL_NOT_FOUND"]);

            if (!OfferedCardIsOwnedByUser(trade.CardToTrade!.Value, UserId))
                return new Response<string>(403, resConfig["TRADE_DEL_ERR"]);

            repo.Delete(trade);

            return new Response<string>(200, resConfig["TRADE_DEL_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove trading deal.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
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

        var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
        var deckCards = cardRepo.GetDeckByUserId(userId.Value);

        if (deckCards == null || deckCards.Count() == 0)
            return false;

        return deckCards.Any<Card>(c => c.Id == cardId);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    private IResponse HandleAcceptUserTradingDeal(Guid tradingdealid)
    {
        throw new NotImplementedException();
    }
}
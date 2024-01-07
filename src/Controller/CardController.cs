using System;
using System.Security.Cryptography.X509Certificates;

namespace MTCG;

/// <summary>
/// Controller for all things card related.
/// </summary>

[Controller]
public class CardController : IController
{
    protected static CardRepository repo = new CardRepository();
    protected CardConfig cardConfig = (CardConfig)ConfigService.Get<CardConfig>()!;
    public CardController(IRequest request) : base(request) { }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Fetches cards in stack for a specific user:
    /// </summary>
    /// <returns></returns>
    [Route("/stack", HTTPMethod.GET, Role.USER)]
    public IResponse GetStackForUser()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            IEnumerable<Card> cards = repo.GetAllCardsInStackByUserId(userId);

            if (cards.Count() == 0) return new Response<string>(204, "The request was fine, but the user doesn't have any cards");

            return new Response<IEnumerable<Card>>(200, cards, $"The user has cards, the response contains these");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, "Internal server error. Something went wrong :()");
        }
    }




    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////








    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/deck", HTTPMethod.GET, Role.USER)]
    public IResponse GetUsersDeck()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            var userCards = repo.GetDeckByUserId(userId);

            if (userCards == null) return new Response<string>(204, resConfig["CRD_DECK_EMPTY"]);

            return new Response<IEnumerable<Card>>(200, userCards, resConfig["CRD_DECK_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/deck", HTTPMethod.PUT, Role.USER)]
    public IResponse AddCardsToDeckByUserId()
    {
        try
        {
            List<Card> providedCards = new();
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            var providedCardIds = request.PayloadAsObject<List<Guid>>();

            if (providedCardIds == null || providedCardIds.Count() < cardConfig.MaxCardsInDeck)
                return new Response<string>(400, resConfig["CRD_DECK_NBR_ERR"]);

            // create cards from ids, bc for whatever reason, the requestbody
            // only contains the ids w/out keys indicating where the value belongs
            foreach (var id in providedCardIds) { providedCards.Add(new Card { Id = id }); }

            if (!IsValidRequestAddCardsToDeck(providedCards, userId))
                return new Response<string>(403, resConfig["CRD_DECK_ADD_ERR"]);

            repo.AddCardsToDeck(providedCards!, userId);
            
            providedCards.ForEach(card => repo.RemoveCardFromStack(card, userId));


            return new Response<string>(200, resConfig["CRD_DECK_ADD_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    protected bool IsValidRequestAddCardsToDeck(IEnumerable<Card> providedCards, Guid userId)
    {
        try
        {
            var userStackCards = repo.GetAllCardsInStackByUserId(userId);
            IEnumerable<Card>? deckCards = repo.GetDeckByUserId(userId);

            var userOwnsProvidedCards = providedCards.All<Card>(pc => userStackCards.Any<Card>(uc => uc.Id == pc.Id));
            var deckIsEmpty = deckCards == null || deckCards.Count() == cardConfig.MaxCardsInDeck;
            // bool cardsLockedInDeck = providedCards.All<Card>(c => !c.Locked);

            return userOwnsProvidedCards && deckIsEmpty;
        }
        catch (Exception ex)
        {
            throw new Exception($"Validating request to add cards to deck failed.\n{ex}");
        }

    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/cards/{cardId:alphanum}", HTTPMethod.GET, Role.ALL)]
    public IResponse GetCardById(Guid cardId)
    {
        try
        {
            Card? card = repo.Get(cardId);

            if (card != null)
            {
                return new Response<Card>(200, card, resConfig["CRD_GETBYID_SUCC"]);
            }
            else
                throw new Exception("Failed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ex");
            
            return new ResponseWithoutPayload(500, $"{resConfig["CRD_GETBYID_ERR"]}\n${ex}");
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    protected void AddCardToStack(Card card, Guid userId)
    {
        try
        {
            repo.AddCardToStack(card, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    [Route("/cards", HTTPMethod.POST, Role.USER)]
    public IResponse AddCardsToStack()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            var cards = request.PayloadAsObject<List<Card>>();
            
            foreach (Card card in cards)
            {
                repo.AddCardToStack(card, userId);
            }

            return new Response<string>(200, resConfig["CRD_STACK_ADD_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add cards to stack.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    [Route("/cards/all", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAllCards()
    {
        try
        {
            IEnumerable<Card> cards = repo.GetAll();

            return new Response<IEnumerable<Card>>(200, cards, "Fetched all cards.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch all cards.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }


    //TODO
    protected Card GetCardByUserId(Guid userId)
    {
        throw new NotImplementedException("");
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    //TODO
    protected bool DeleteCardFromStack(Guid cardId, Guid userId)
    {
        throw new NotImplementedException("");
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    //TODO
    public IResponse BuyCard()
    {
        throw new NotImplementedException("");
        // try
        // {
        //     Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
        //     var package = request.PayloadAsObject<IEnumerable<Card>>();

        //     var cards = repo.BuyCard(userId, package);

        //     return new Response<IEnumerable<Card>>(200, cards, $"The user has cards, the response contains these");
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine(ex);

        //     return new Response<string>(500, "Internal server error. Something went wrong :()");
        // }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    // TODO
    protected bool CardExists(Card card)
    {
        throw new NotImplementedException("");
    }

}


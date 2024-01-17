using System;
using System.Security.Cryptography.X509Certificates;
using Npgsql;

namespace MTCG;

/// <summary>
/// Controller for all things card related.
/// </summary>

[Controller]
public class CardController : IController
{
    protected CardRepository repo = ServiceProvider.GetFreshInstance<CardRepository>();
    protected CardConfig cardConfig = ServiceProvider.Get<CardConfig>();
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

            if (cards.Count() == 0) return new Response<string>(204, resConfig["CRD_DECK_EMPTY"]);

            return new Response<IEnumerable<Card>>(200, cards, resConfig["CRD_DECK_SUCC"]);
        }
        catch (Exception ex)
        {
            Logger.Err(ex, true);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/deck", HTTPMethod.GET, Role.USER)]
    public IResponse GetUsersDeck()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            var userCards = repo.GetDeckByUserId(userId);

            if (userCards == null || userCards.Count() == 0) return new Response<string>(204, resConfig["CRD_DECK_EMPTY"]);

            return new Response<IEnumerable<DeckCard>>(200, userCards, resConfig["CRD_DECK_SUCC"]);
        }
        catch (Exception ex)
        {
            Logger.Err(ex, true);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/cards", HTTPMethod.DELETE, Role.ADMIN)]
    public IResponse DeleteAllCardsGlobally()
    {
        try
        {
            repo.DeleteAll();

            return new Response<string>(200, "Successfully deleted all cards.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete all cards.\n{ex}");

            return new Response<string>(500, "Failed to delete all cards globally.");
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
            var providedCardIds = request.PayloadAsObject<List<Guid>>();

            if (providedCardIds == null || providedCardIds.Count() != cardConfig.MaxCardsInDeck)
                return new Response<string>(400, resConfig["CRD_DECK_NBR_ERR"]);

            // create cards from ids, bc for whatever reason, the requestbody
            // only contains the ids w/out keys indicating where the value belongs
            foreach (var id in providedCardIds) { providedCards.Add(new Card { Id = id }); }

            if (!IsValidRequestAddCardsToDeck(providedCards, UserId))
                return new Response<string>(403, resConfig["CRD_DECK_ADD_ERR"]);

            repo.AddCardsToDeck(providedCards!, UserId);

            providedCards.ForEach(card => repo.RemoveCardFromStack(card, UserId));


            return new Response<string>(200, resConfig["CRD_DECK_ADD_SUCC"]);
        }
        catch (Exception ex)
        {
            Logger.Err(ex, true);

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
            bool userOwnsProvidedCards = true;
            // var userOwnsProvidedCards = providedCards.All<Card>(pc => userStackCards.Any<Card>(uc => uc.Id == pc.Id));
            foreach (var stackCard in userStackCards)
            {
                userOwnsProvidedCards = providedCards.Any<Card>(c => c.Id == stackCard.Id) && userOwnsProvidedCards;
            }
            var deckIsEmpty = deckCards == null || deckCards.Count() == cardConfig.MinCardsInDeck;

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
                return new Response<Card>(200, card, resConfig["CRD_GETBYID_SUCC"]);
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
            Logger.Err(ex, true);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/stack/{userid:alphanum}", HTTPMethod.POST, Role.ADMIN)] // CHANGED from /cards -> /stack
    public IResponse AddCardsToStack(Guid userid)
    {
        try
        {
            var userRepo = ServiceProvider.GetFreshInstance<UserRepository>();
            var user = userRepo.Get(userid);

            if (user == null) return new Response<string>(204, "User not found.");

            var userId = user.ID;

            var cards = request.PayloadAsObject<List<Card>>();

            if (cards == null || cards.Count() == 0)
                return new Response<string>(500, resConfig["CRD_STACK_ADD_ERR"]);

            foreach (Card card in cards)
                repo.AddCardToStack(card, userId);

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



    [Route("/cards", HTTPMethod.POST, Role.ADMIN)]
    public IResponse AddCard()
    {
        Card? card = null;
        try
        {
            card = request.PayloadAsObject<Card>();

            if (card == null) return new Response<string>(400, "No card provided.");

            repo.Save(card);

            return new Response<string>(200, "Created card.");
        }
        catch (PostgresException pex)
        {
            if (pex.SqlState == "23505")
            {
                Console.WriteLine($"Card {card!.Name} already in DB.");

                return new Response<string>(200, "Created card.");
            }
            else
                throw pex;

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add card.\n{ex}");

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
}


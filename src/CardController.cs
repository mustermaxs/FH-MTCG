using System;

namespace MTCG;

[Controller]
public class CardController : IController
{
    protected static CardRepository repo = new CardRepository();
    public CardController(IRequest request) : base(request) { }

    [Route("/cards", HTTPMethod.GET, Role.USER)]
    public IResponse GetCardsForUser()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            IEnumerable<Card> cards = repo.GetAllByUserId(userId);

            return new Response<IEnumerable<Card>>(200, cards, $"The user has cards, the response contains these");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, "Internal server error. Something went wrong :()");
        }
    }

    [Route("/packages", HTTPMethod.POST, Role.ADMIN)]
    public IResponse AddPackage()
    {
        try
        {
            List<Card>? cards = request.PayloadAsObject<List<Card>>();
            
            if (cards == null || cards.Count < 5) return new Response<string>(400, "Package must consist of 5 cards");
            
            repo.AddPackage(cards);

            return new Response<string>(200, "Package and cards successfully created");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new Response<string>(403, "At least one card in the packages already exists");
        }
    }

    [Route("/deck", HTTPMethod.GET, Role.USER)]
    public IResponse CreateDeckForUser()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            var userCards = repo.GetDeckByUserId(userId);

            if (userCards == null) return new Response<string>(204, "The request was fine, but the deck doesn't have any cards");

            return new Response<IEnumerable<Card>>(200, userCards, "The deck has cards, the response contains these");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, "Something went wrong :(");
        }
    }

    [Route("/deck", HTTPMethod.PUT, Role.USER)]
    public IResponse AddCardsToDeckByUserId()
    {
        try
        {
            Guid userId = SessionManager.GetUserBySessionId(request.SessionId).ID;
            var cards = request.PayloadAsObject<IEnumerable<Card>>();
            // TODO
            // repo.AddCardsToDeck
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, "Something went wrong :(");
        }
    }



    [Route("/cards/{cardId:alphanum}", HTTPMethod.GET, Role.ALL)]
    public IResponse GetCardById(Guid cardId)
    {
        try
        {
            Card? card = repo.Get(cardId);

            if (card != null)
            {
                return new Response<Card>(200, card, "Fetched card.");
            }
            else
                throw new Exception("Failed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ex");
            return new ResponseWithoutPayload(500, $"Failed to fetch card.\n${ex}");
        }
    }

    public void AddCardToStack(Card card, Guid userId)
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

    //TODO
    protected Card GetCardByUserId(Guid userId)
    {
        throw new NotImplementedException("");
    }

    //TODO
    protected bool DeleteCardFromStack(Guid cardId, Guid userId)
    {
        throw new NotImplementedException("");
    }

    //TODO
    [Route("/transactions/packages", HTTPMethod.POST, Role.USER | Role.ADMIN)]
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

    // TODO
    protected bool CardExists(Card card)
    {
        throw new NotImplementedException("");
    }

}


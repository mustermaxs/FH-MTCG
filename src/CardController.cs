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

    [Route("/packages", HTTPMethod.POST, Role.ALL)]
    public IResponse AddPackage()
    {
        try
        {
            
            List<Card>? cards = request.PayloadAsObject<List<Card>>();

            if (cards == null) return new Response<string>(400, "Package must consist of 5 cards.");
            
            return new Response<IEnumerable<Card>>(200, cards, "It worked.");


            // repo.SavePackage(cards);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new Response<string>(500, "It failed :(");

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


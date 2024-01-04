using System;

namespace MTCG;


[Controller]
public class PackageController : IController
{
    protected static PackageRepository repo = new PackageRepository();
    public PackageController(IRequest request) : base(request) { }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    [Route("/packages", HTTPMethod.POST, Role.USER)]
    public IResponse AddPackage()
    {
        try
        {
            List<Card>? cards = request.PayloadAsObject<List<Card>>();
            var cardRepo = new CardRepository();
            List<Guid> addedCardIds = new List<Guid>();

            if (cards == null || cards.Count < 5) return new Response<string>(400, "Package must consist of 5 cards");

            cards.ForEach(card => card.Id = cardRepo.SaveAndGetInsertedId(card));
            var package = new Package();
            package.Cards = cards;

            repo.Save(package);

            return new Response<string>(200, "Package and cards successfully created");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(403, "At least one card in the packages already exists");
        }
    }
}
//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

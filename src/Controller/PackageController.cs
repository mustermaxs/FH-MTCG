using System;

namespace MTCG;


[Controller]
public class PackageController : IController
{
    protected static PackageRepository repo = new PackageRepository();
    public PackageController(IRequest request) : base(request) { }
    const int MIN_COINS_NORMAL_PACKAGE = 5;

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    [Route("/packages", HTTPMethod.POST, Role.USER)] // CHANGE only ADMIN
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


    [Route("/packages", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAvailablePackages()
    {
        try
        {
            IEnumerable<Package> packages = repo.GetAll();

            if (packages.Count() == 0) return new Response<string>(204, "The request was fine, but there are no packages");

            return new Response<IEnumerable<Package>>(200, packages, $"The response contains all packages");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, "Internal server error. Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    [Route("/packages/{packageid:alphanum}", HTTPMethod.GET, Role.ALL)]
    public IResponse GetPackageById(Guid packageid)
    {
        try
        {
            Package? package = repo.Get(packageid);

            if (package == null) return new Response<string>(204, "The request was fine, but the package doesn't exist");

            return new Response<Package>(200, package, $"The response contains the package");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, "Internal server error. Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    
    [Route("/transactions/packages", HTTPMethod.POST, Role.USER)]
    public IResponse UserBuysPackage()
    {
        throw new NotImplementedException();
        // var user = LoggedInUser;
        // Guid? packageId = repo.GetAllPackageIds().FirstOrDefault();

        // if (packageId == Guid.Empty || packageId == null)
        //     return new Response<string>(404, "No card package available for buying");
        
        // if (user.Coins >= 5)
        
    }
}
//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

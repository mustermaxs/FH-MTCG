using System;

namespace MTCG;


[Controller]
public class PackageController : IController
{
    protected static PackageRepository repo = new PackageRepository();
    protected CardConfig config = (CardConfig)ConfigService.Get<CardConfig>();
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

            if (cards == null || cards.Count < config.ReqNbrCardsInPackage) return new Response<string>(400, "Package must consist of 5 cards");

            cards.ForEach(card => card.Id = cardRepo.SaveAndGetInsertedId(card));
            var package = new Package();
            package.Cards = cards;

            repo.Save(package);

            return new Response<string>(200, resConfig["PCK_ADD_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(403, resConfig["PCK_ADD_EXISTS"]);
        }
    }


    [Route("/packages", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAvailablePackages()
    {
        try
        {
            IEnumerable<Package> packages = repo.GetAll();

            if (packages.Count() == 0) return new Response<string>(204, resConfig["PCK_GETALL_NO_PCKS"]);

            return new Response<IEnumerable<Package>>(200, packages, resConfig["PCK_GETALL_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
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

            if (package == null) return new Response<string>(204, resConfig["PCK_REQ_OK_NOTEXISTS"]);

            return new Response<Package>(200, package, resConfig["PCK_BYID_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
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

    [Route("/packages/{id:alphanum}", HTTPMethod.DELETE, Role.ADMIN)]
    public IResponse DeletePackage(Guid id)
    {
        try
        {
            var package = repo.Get(id);

            if (package == null) return new Response<string>(400, resConfig["PCK_REQ_OK_NOTEXISTS"]);
            
            repo.Delete(package);

            return new Response<string>(200, "Package was deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }
}
//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

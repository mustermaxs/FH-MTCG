using System;
using System.Security.Cryptography;
using Npgsql;


namespace MTCG;


[Controller]
public class BattleController : IController
{
    protected BattleRepository battleRepo = ServiceProvider.GetDisposable<BattleRepository>();
    protected BattleLogRepository battleLogRepo = ServiceProvider.GetDisposable<BattleLogRepository>();
    protected BattleService battleService = new BattleService();
    public BattleController(IRequest request) : base(request) { }



    [Route("/battle", HTTPMethod.POST, Role.USER)]
    public async Task<IResponse> AddBattleRequest()
    {
        Battle? battle = null;

        try
        {
            var language = LoggedInUser.Language;

            var cardRepo = ServiceProvider.GetDisposable<CardRepository>();
            string res = string.Empty;
            battle = await BattleService.BattleRequest(LoggedInUser);
            battleRepo.Save(battle);
            var battlePrinter = new BattlePrintService();
            var battleConfig = Program.services.Get<BattleConfig>();
            battleConfig.SetLanguage(language);
            battlePrinter.battleConfig = battleConfig;
            string battleResultTxt = battlePrinter.GetBattleLogAsTxt(battle);

            Console.WriteLine("Saved battle to DB.");

            return new Response<string>(200, battleResultTxt, "BATTLE SUCCESSFUL");
        }
        catch (PostgresException pex)
        {
            Console.WriteLine($"Battle already saved to DB by other player.\n{pex}");

            return new Response<Battle>(200, battle, "BATTLE SUCCESSFUL");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, ex.Message, "BATTLE FAILED");
        }
    }


    [Route("/battle", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAll()
    {
        try
        {
            var battles = battleRepo.GetAll();

            return new Response<IEnumerable<Battle>>(200, battles, "Battles");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }

    // protected void SaveBattle(Battle battle)
    // {
    //     try
    //     {
    //         battleRepo.Save(battle);
    //     }
    // catch (PostgresException pex)
    // {

    // }
    //     catch (Exception ex)
    //     {

    //     }
    // }
}
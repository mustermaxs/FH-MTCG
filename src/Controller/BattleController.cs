using System;
using System.Security.Cryptography;
using Npgsql;


namespace MTCG;


[Controller]
public class BattleController : IController
{
    protected BattleRepository battleRepo = ServiceProvider.GetDisposable<BattleRepository>();
    protected BattleLogRepository battleLogRepo = ServiceProvider.GetDisposable<BattleLogRepository>();
    protected BattleWaitingRoomManager battleService = new BattleWaitingRoomManager();
    public BattleController(IRequest request) : base(request) { }



    [Route("/battle", HTTPMethod.POST, Role.USER)]
    public async Task<IResponse> HandleBattleRequest()
    {
        Battle? battle = null;

        try
        {
            var userRepo = ServiceProvider.GetDisposable<UserRepository>();
            var battlePrinter = new BattlePrintService();
            var battleConfig = Program.services.Get<BattleConfig>().Load<BattleConfig>();
            battleConfig.SetLanguage(LoggedInUser.Language); // sollte nicht immer explizit gesettet werden müssen, sollte "autom." passieren

            battle = await BattleWaitingRoomManager.BattleRequest(LoggedInUser);
            battleRepo.Save(battle);
            var currentUser = battle.Player1!.ID == LoggedInUser.ID ? battle.Player1 : battle.Player2;
            userRepo.Update(currentUser);

            battlePrinter.battleConfig = battleConfig;

            return new Response<string>(200, battlePrinter.GetBattleLogAsTxt(battle), "BATTLE SUCCESSFUL");
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


    [Route("/stats", HTTPMethod.GET, Role.USER)]
    public IResponse GetStats()
    {
        try
        {
            var stats = battleRepo.GetUserBattleStats(UserId);

            return new Response<Dictionary<string,dynamic>>(200, stats, "Stats");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    
    [Route("/scoreboard", HTTPMethod.GET, Role.USER | Role.ADMIN)]
    public IResponse GetScoreboard()
    {
        try
        {
            var stats = battleRepo.GetAllStats();

            return new Response<List<Dictionary<string, dynamic>>>(200, stats, "Scoreboard");
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
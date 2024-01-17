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
    protected UserRepository userRepo = ServiceProvider.GetDisposable<UserRepository>();


    public BattleController(IRequest request) : base(request) { }




    [Route("/battle", HTTPMethod.POST, Role.USER)]
    public async Task<IResponse> HandleBattleRequest()
    {
        Battle? battle = null;

        try
        {
            var battlePrinter = new BattlePrintService();
            var battleConfig = Program.services.Get<BattleConfig>().Load<BattleConfig>();
            battleConfig.SetLanguage(LoggedInUser.Language); // sollte nicht immer explizit gesettet werden müssen, sollte "autom." passieren

            battle = await BattleWaitingRoomManager.BattleRequest(LoggedInUser);
            battleRepo.Save(battle);
            userRepo.Update(LoggedInUser);

            battlePrinter.battleConfig = battleConfig;

            return new Response<string>(200, battlePrinter.GetBattleLogAsTxt(battle), resConfig["BATTLE_SUCC"]);
        }


        
        catch (PostgresException pex)
        {
            if (pex.SqlState != "2305") throw;

            Console.WriteLine($"Battle already saved to DB by other player.\n{pex}");

            return new Response<Battle>(200, battle, resConfig["BATTLE_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, ex.Message, resConfig["INT_SVR_ERR"]);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/battle", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAll()
    {
        try
        {
            var battles = battleRepo.GetAll();

            return new Response<IEnumerable<Battle>>(200, battles, "Got all battles");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/stats", HTTPMethod.GET, Role.USER)]
    public IResponse GetStats()
    {
        try
        {
            var stats = battleRepo.GetUserBattleStats(UserId);

            return new Response<Dictionary<string,dynamic>>(200, stats, resConfig["STATS_SUCC"]);
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

            return new Response<List<Dictionary<string, dynamic>>>(200, stats, resConfig["SCORE_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }
}
using System;

namespace MTCG;


[Controller]
public class BattleController : IController
{
    // protected static BattleRepository repo = new BattleRepository();
    protected BattleService battleService = new BattleService();
    public BattleController(IRequest request) : base(request) { }

    [Route("/battle", HTTPMethod.POST, Role.USER)]
    public async Task<IResponse> AddBattleRequest()
    {
        try
        {
            string res = string.Empty;
            var battleRes = await BattleService.BattleRequest(LoggedInUser);

            return new Response<Battle>(200, battleRes, "BATTLE SUCCESSFUL");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, ex.Message, "BATTLE FAILED");
        }
    }

    // protected async Task<string> Foo()
    // {
    //     return await BattleService.HandleBattle(LoggedInUser);
    // }
}
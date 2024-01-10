using System;

namespace MTCG;


[Controller]
public class BattleController : IController
{
    // protected static BattleRepository repo = new BattleRepository();
    protected BattleService battleService = new BattleService();
    public BattleController(IRequest request) : base(request) { }

    [Route("/battle", HTTPMethod.POST, Role.USER)]
    public IResponse AddBattleRequest()
    {
        try
        {
            string res = await BattleService.HandleBattle(LoggedInUser);
            
            Console.WriteLine(res);
            
            return new Response<string>(200, res, "BATTLE");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, ex.Message, "BATTLE FAILED");
        }
    }

    protected async Task<string> Foo()
    {
        return await BattleService.HandleBattle(LoggedInUser);
    }
}
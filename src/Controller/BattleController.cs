using System;

namespace MTCG;

public class BattleController : IController
{
    // protected static BattleRepository repo = new BattleRepository();

    public BattleController(IRequest request) : base(request) { }


    public IResponse AddBattleToQueue()
    {
        try
        {
            return new Response<string>(200, "Battle added to queue");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }
       
}
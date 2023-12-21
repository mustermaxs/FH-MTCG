using System;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


public class TradingController : IController
{
    protected static TradingRepository repo = new TradingRepository();
    public TradingController(IRequest request) : base(request) { }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/tradings", HTTPMethod.GET, Role.ALL)]
    public IResponse GetAllTradingDeals()
    {
        throw new NotImplementedException("Not implemented.");
        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to fetch all pending trading deals.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/tradings", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse AddTradingDeal()
    {
        throw new NotImplementedException("Not implemented.");

        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to add new trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }  
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    
    
    [Route("/tradings/{tradingdealid:int}", HTTPMethod.DELETE, Role.USER | Role.ADMIN)]
    public IResponse DeleteTradingDeal(int tradingdealid)
    {
        throw new NotImplementedException("Not implemented.");

        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to remove trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }    
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    

    [Route("/tradings/{tradingdealid:int}", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse HandleAcceptTradingDeal(int tradingdealid)
    {
        throw new NotImplementedException("Not implemented.");

        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to accept trading deal.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }  
    }
}
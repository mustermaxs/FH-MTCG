using System.Text.Json.Serialization;

namespace MTCG;

public interface IModel
{
    
}

class Request
{
    /// IMPORTANT
    /// include payload (JSON)
    public Request() { }
    private HTTPMethod method;
    private IModel payload;
    public IModel Payload => payload;
    private int status = 0;
    
}
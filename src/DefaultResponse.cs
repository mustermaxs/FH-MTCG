using System;

namespace MTCG;

/// 06.12.2023 21:45
/// OBSOLETE

public class Response : IResponse
{
    public int StatusCode { get; }
    public string Description {get;}

    public Response(int statusCode, string description)
    {
        this.StatusCode = statusCode;
        this.Description = description;
    }


    virtual public string PayloadAsJson() => string.Empty;
}
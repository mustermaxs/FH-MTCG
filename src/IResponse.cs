using System;

namespace MTCG;

public abstract class IResponse
{
    public IResponse(int statusCode, string? payload, string description)
    {
        this.Payload = payload ?? string.Empty;
        this.statusCode = statusCode;
        this.Description = description;
    }
    protected int statusCode;

    public string Description {get; set;}
    public int Status => statusCode;
    virtual public string Payload {get;} = string.Empty;
}
using System;

namespace MTCG;

public class ResponseWithoutPayload : IResponse
{
    public ResponseWithoutPayload(int status, string description)
    {
        this.statusCode = status;
        this.description = description;
    }
    
    private int statusCode;
    private string description;
    public string ContentType { get; } = "text/html";
    public int StatusCode { get => statusCode; }
    public string Description { get => description; }
    public string PayloadAsJson() => string.Empty;   
}
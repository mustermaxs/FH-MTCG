using System;
using System.Text.Json.Serialization;

namespace MTCG;

public class Request : IRequest
{
    public Request(string payload)
    {
        this.payload = payload;
    }
}
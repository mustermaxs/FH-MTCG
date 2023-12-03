using System;

namespace MTCG;

public class Response : IResponse
{
    public Response(int statusCode, string payload, string text)
    :base(statusCode, payload, text)
    {
    }
}
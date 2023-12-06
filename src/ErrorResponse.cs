using System;
using System.Text.Json;

namespace MTCG;

public class ErrorResponse<T> : PayloadResponse<T>, IResponse where T : class?
{
    public ErrorResponse(string description, int statusCode = 500)
    :base(statusCode, null, description)
    {
    }
}
using System;
using System.Text.Json;

namespace MTCG;

public class Response<T> : BaseJsonResponse<T> where T : class?
{
    public Response(int statusCode, T? payload, string description)
        : base(statusCode, payload, description)
    {
    }

        public Response(int statusCode, string description)
        : base(statusCode, description)
    {
    }
}

using System;
using System.Text.Json;

namespace MTCG;


public class Response<T> : BaseJsonResponse<T> where T : class?
{

    /// <summary>
    /// Base class for all responses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public Response(int statusCode, T? payload, string description)
        : base(statusCode, payload, description)
    {
    }

    /// <summary>
    /// Base class for all responses.
    /// </summary>
    /// <param name="statusCode">
    /// 200: OK
    /// ...
    /// </param>
    /// <param name="description">
    /// Some descriptive text explaining the response imore detail.
    /// </param>
    public Response(int statusCode, string description)
    : base(statusCode, description)
    {
    }
}

using System;
using System.Text.Json;

namespace MTCG;

public class SuccessResponse<T> : BaseJsonResponse<T> where T : class?
{
    public SuccessResponse(int statusCode, T? payload, string description)
        : base(statusCode, payload, description)
    {
    }
}

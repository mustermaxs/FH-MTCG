using System;
using System.Text.Json;

namespace MTCG;

public class CustomResponse<T> : BaseResponse<T> where T : class?
{
    public CustomResponse(int statusCode, T payload, string description)
    :base(statusCode, payload, description)
    {
    }
}
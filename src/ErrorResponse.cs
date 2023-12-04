using System;
using System.Text.Json;

namespace MTCG;

public class ErrorResponse<T> : BaseResponse<T>, IResponse where T : class?
{
    public ErrorResponse(T payload, string description)
    :base(500, payload, description)
    {
    }
}
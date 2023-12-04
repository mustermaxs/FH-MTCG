using System;
using System.Text.Json;

namespace MTCG;

public class SuccessResponse<T> : BaseResponse<T> where T : class?
{
    public SuccessResponse(T payload, string description)
        : base(200, payload, description)
    {
    }
}
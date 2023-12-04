using System;

namespace MTCG;

public interface IResponse
{
    int StatusCode { get; }
    string Description { get; }
    string PayloadAsJson();
}
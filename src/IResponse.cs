using System;
using System.Net;

namespace MTCG;

public abstract class IResponse<T>
{
    protected bool success = false;
    protected T payload;
    protected HttpCode statusCode;
    public IResponse(HttpCode statusCode, T payload)
    {
        this.payload = payload;
        this.statusCode = statusCode;
    }
    public bool Successful => success;
    public HttpCode StatusCode => statusCode;

    virtual public T Payload
    {
        get => this.payload;
    }
}
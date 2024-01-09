using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace MTCG;

/// <summary>
/// Base class for all JSON responses.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseJsonResponse<T> : IResponse where T : class?
{
    public T? Payload { get; protected set; }
    virtual public int StatusCode { get; protected set; }
    virtual public string Description { get; protected set; }
    virtual public string ContentType { get; } = "application/json";


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public BaseJsonResponse(int statusCode, T? payload, string? description)
    {
        this.Payload = payload;
        this.StatusCode = statusCode;
        this.Description = description ?? string.Empty;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public BaseJsonResponse(int statusCode, string? description)
    {
        this.Payload = null;
        this.StatusCode = statusCode;
        this.Description = description ?? string.Empty;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    virtual public BaseJsonResponse<T> SetPayload(T payload)
    {
        this.Payload = payload;

        return this;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    protected virtual string HandleEnumerablePayload()
    {
        var list = new List<object>();

        foreach (var item in Payload as IEnumerable<IModel>)
        {
            list.Add(item.ToSerializableObj());
        }

        return JsonConvert.SerializeObject(list);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    virtual public string PayloadAsJson()
    {
        if (Payload == null)
        {
            return string.Empty;
        }

        if (Payload is IModel)
            return JsonConvert.SerializeObject((Payload as IModel)?.ToSerializableObj());

        if (Payload is IEnumerable<IModel>)
            return HandleEnumerablePayload();

        return JsonConvert.SerializeObject(Payload);
    }
}
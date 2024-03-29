using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// using Newtonsoft.Json;
// using Newtonsoft.Json.Converters;


namespace MTCG;

/// <summary>
/// Wrapper to store named params and their
/// corresponding values (among other things).
/// </summary>

public class Request : IRequest
{

    public Request(IEndpoint endpoint)
    {
        this.routeFound = false;
        this.sessionId = string.Empty;
    }
    private string? sessionId;
    public string? SessionId { get => sessionId ?? string.Empty; }

    public Request(HTTPMethod method, string? rawUrl, HttpHeader[]? headers, string? payload, string? sessionId)
    {
        this.routeFound = false;
        this.HttpMethod = method;
        this.rawUrl = rawUrl;
        this.Headers = headers;
        this.Payload = payload ?? string.Empty;
        this.sessionId = sessionId ?? string.Empty;
    }

    public IRequest SetPayload(string payload)
    {
        this.Payload = payload;

        return this;
    }

    // private HTTPMethod GetHttpMethodFromHeaders(HttpHeader[] headers)
    // {
    //     var headerHttpMethod = Array.Find(headers, header => header.Name == "Method");

    //     if (headerHttpMethod is null)
    //     {
    //         throw new Exception("No HTTP method provided.");
    //     }

    //     if (Enum.TryParse(headerHttpMethod.Value, out HTTPMethod method))
    //     {
    //         return method;
    //     }
    //     else
    //     {
    //         throw new Exception($"Invalid HTTP method: {headerHttpMethod.Value}");
    //     }
    // }


    public Dictionary<string, string> UrlParams
    {
        get { return urlParams; }
        set { urlParams = value; }
    }




    public bool TryGetHeader(string key, out string value)
    {
        value = this.Headers.SingleOrDefault<HttpHeader>(header => header.Name == key)?.Value ?? string.Empty;

        return value != string.Empty;
    }




    /// <summary>
    /// Gets value of named parameter by parameter name and tries to convert it to 
    /// expected datatype.
    /// </summary>
    /// <returns>
    /// Value of named parameter. Throws KeyNotFoundException if it doesn't exist. 
    /// Throws InvalidOperationException if it's tried to be cast to invalid datatype
    /// </returns>
    /// IMPROVE GetParam methode verwenden um value zu konvertieren
    /// und NICHT die von CustomReflectionExtension
    public T GetParam<T>(string key)
    {
        string? value;

        if (!routeFound) { throw new Exception("Route not registered."); }

        if (this.urlParams.TryGetValue(key, out value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting parameter to type {typeof(T)}: {ex}");
            }
        }

        throw new KeyNotFoundException($"Parameter {key} couldn't be found");
    }

    public bool SetParam(string key, string value)
    {
        if (!urlParams.TryGetValue(key, out _))
        {
            urlParams[key] = value;

            return true;
        }

        return false;
    }

    private Dictionary<string, string> urlParams = new Dictionary<string, string>();
    protected string? rawUrl;
    protected bool routeFound = false;
    public bool RouteFound { get => routeFound; set { routeFound = value; } }
    public HTTPMethod HttpMethod { get; protected set; }

    /// 12.02.2023 21:13
    /// FIXME default value
    public virtual HttpHeader[]? Headers { get; set; }
    public IEndpoint? Endpoint { get; set; }
    public string? RawUrl { get => rawUrl; }
    public string? Payload
    {
        get;
        set;
    }

    public T? PayloadAsObject<T>() where T : class
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
        return !string.IsNullOrEmpty(Payload) ? JsonSerializer.Deserialize<T>(Payload, options) : null;
    }

    [Obsolete("Könnt ich eig gleich löschen... aber vlt brauch ichs noch")]
    public IEnumerable<T>? PayloadAsEnumerable<T>() where T : class
    {
        return !string.IsNullOrEmpty(Payload) ? JsonSerializer.Deserialize<IEnumerable<T>>(Payload) : null;
    }

    [Obsolete("Könnt ich eig gleich löschen... aber vlt brauch ichs noch")]
    public List<T>? PayloadAsList<T>() where T : class
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
        return !string.IsNullOrEmpty(Payload) ? JsonSerializer.Deserialize<List<T>>(Payload, options) : null;
    }
}

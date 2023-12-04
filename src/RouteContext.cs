namespace MTCG;

/// <summary>
/// Wrapper to store named params and theirs
/// corresponding values (among other things).
/// </summary>

public class RoutingContext : IRoutingContext
{

    public RoutingContext(IEndpoint endpoint)
    {
        this.routeFound = false;
    }
    public RoutingContext(HTTPMethod method, string rawUrl)
    {
        this.httpMethod = method;
        this.routeFound = false;
        this.rawUrl = rawUrl;
    }
    public RoutingContext(string rawUrl)
    {
        this.routeFound = false;
    }
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

        if (!routeFound)
        {
            throw new Exception("Route not registered.");
        }

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

    public HTTPMethod httpMethod { get; protected set; }

    protected bool routeFound = false;

    public bool RouteFound
    {
        get => routeFound;
        set { routeFound = value; }
    }

    public HTTPMethod Method => this.httpMethod;

    /// 12.02.2023 21:13
    /// FIXME default value
    public virtual HttpHeader[] Headers { get; set; } = new HttpHeader[0];


    public IEndpoint? Endpoint
    {
        get; set;
    }
    public string? RawUrl { get => rawUrl; }
    public string Payload { get; set; }
}

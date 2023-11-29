namespace MTCG;

/// <summary>
/// Wrapper to store named params and theirs
/// corresponding values (among other things).
/// </summary>

public class RequestObject
{
    private Dictionary<string, string> urlParams = new Dictionary<string, string>();
    private string? rawUrl;
    private HTTPMethod method;
    bool routeFound = false;
    private AbstractEndpoint? endpoint;
    public RequestObject(Dictionary<string, string> parameters, AbstractEndpoint endpoint)
    {
        this.urlParams = parameters;
        this.endpoint = endpoint;
        this.routeFound = true;
    }
    public RequestObject()
    {
        this.routeFound = false;
    }
    public Dictionary<string, string> UrlParams
    {
        private get { return urlParams; }
        set { urlParams = value; }
    }

    public bool RouteFound => routeFound;
    public RequestObject(HTTPMethod method, string rawUrl)
    {
        this.routeFound = false;
    }
    public RequestObject(string rawUrl)
    {
        this.routeFound = false;
    }
    public HTTPMethod Method
    {
        get => this.method;
    }
    public string? RawUrl
    {
        get => this.rawUrl;
    }

    /// <summary>
    /// Gets value of named parameter by parameter name and tries to convert it to 
    /// expected datatype.
    /// </summary>
    /// <returns>
    /// Value of named parameter. Throws KeyNotFoundException if it doesn't exist. 
    /// Throws InvalidOperationException if it's tried to be cast to invalid datatype
    /// </returns>
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

}

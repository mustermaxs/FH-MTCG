namespace MTCG;

/// <summary>
/// Wrapper to store named params and theirs
/// corresponding values (among other things).
/// </summary>

public class ResolvedUrl
{
    private Dictionary<string, string> urlParams = new Dictionary<string, string>();
    private string? rawUrl;
    private HTTPMethod method;
    bool isRouteRegistered;
    public ResolvedUrl(Dictionary<string, string> parameters, HTTPMethod method, string rawUrl)
    {
        this.urlParams = parameters;
        this.method = method;
        this.rawUrl = rawUrl;
    }
    public bool IsRouteRegistered { get; set; } = false;
    public Dictionary<string, string> UrlParams
    {
        private get { return urlParams; }
        set { urlParams = value; }
    }

    public ResolvedUrl(HTTPMethod method, string rawUrl)
    {
        this.isRouteRegistered = false;
    }
    public ResolvedUrl(string rawUrl)
    {
        this.isRouteRegistered = false;
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

}

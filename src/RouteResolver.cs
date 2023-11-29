using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

/// <summary>Works as a store for registered routes. Can be used to check if requested route matches
/// any of the registered route patterns. If so, it can provide a ResolveRoute object containing the 
/// named parameters and their associated values</summary>
public class RouteResolver
{
    public Dictionary<HTTPMethod, List<string>> parsedRouteTemplates;
    private IUrlParser urlParser;

    /// <summary>Constructor</summary>
    /// <param name="urlParser">Requires a parser that implements the interface IUrlParser</param>
    public RouteResolver(IUrlParser urlParser)
    {
        this.urlParser = urlParser;
        parsedRouteTemplates = new Dictionary<HTTPMethod, List<string>>
        {
          { HTTPMethod.GET, new List<string>() },
          { HTTPMethod.POST, new List<string>() },
          { HTTPMethod.PUT, new List<string>() },
          { HTTPMethod.DELETE, new List<string>() },
          { HTTPMethod.PATCH, new List<string>() }
        };
    }

    /// <summary>Register a route template pattern using HTTP method GET</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    public void RegisterGet(string routePattern)
    {
        this.parsedRouteTemplates[HTTPMethod.GET].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }
    /// <summary>Register a route template pattern using HTTP method POST</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    public void RegisterPost(string routePattern)
    {
        this.parsedRouteTemplates[HTTPMethod.POST].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }
    /// <summary>Register a route template pattern using HTTP method PUT</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    public void RegisterPut(string routePattern)
    {
        this.parsedRouteTemplates[HTTPMethod.PUT].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }
    /// <summary>Register a route template pattern using HTTP method DELETE</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    public void RegisterDelete(string routePattern)
    {
        this.parsedRouteTemplates[HTTPMethod.DELETE].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }

    /// <summary>Register a route template pattern.</summary>
    /// <param name="method">Expects the HTTP method for which the route is registered for.</param>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    public void RegisterRoute(string routePattern, HTTPMethod method)
    {
        if (Enum.IsDefined<HTTPMethod>(method))
        {
            this.parsedRouteTemplates[method].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
        }
        else
        {
            throw new ArgumentException($"Unkown HTTP Method provided. Acceptable methods are: {HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }

    }

    // TODO passenden Controller & Methode finden
    // wer soll das machen?
    // was soll Map eigentlich genau machen?
    // IMPROVE potentialPatterns umbenennen
    // ? was soll diese methode überhaupt machen
    /// <summary>Checks if the requested route is registered in the store.</summary>
    /// <returns>ResolvedUrl object containing (in case they exist) values of the named url parameters 
    /// and a bool (IsRouteRegistered) inidicating if the requested route was even registered in the store.</returns>
    public ResolvedUrl? MapRequest(string requestedUrl, HTTPMethod method)
    {
        Dictionary<string, string> namedTokensInUrlFound = new();
        List<string>? potentialPatterns;
        var res = new ResolvedUrl(method, requestedUrl);

        if (!parsedRouteTemplates.TryGetValue(method, out potentialPatterns))
            return new ResolvedUrl(method, requestedUrl);

        string trimmedUrl = urlParser.CleanUrl(requestedUrl);


        foreach (string pattern in potentialPatterns)
        {
            namedTokensInUrlFound = this.urlParser.MatchUrlAndGetParams(trimmedUrl, pattern);

            if (namedTokensInUrlFound.Count > 0)
            {
                res.UrlParams = namedTokensInUrlFound;
                res.IsRouteRegistered = true;
                break;
            }
        }

        return res;
    }

}
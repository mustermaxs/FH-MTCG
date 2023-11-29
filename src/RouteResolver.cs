using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG;

/// <summary>Works as a store for registered routes. Can be used to check if requested route matches
/// any of the registered route patterns. If so, it can provide a ResolveRoute object containing the 
/// named parameters and their associated values</summary>
public class EndpointMapper : IEndpointMapper
{
    private Dictionary<HTTPMethod, List<string>> parsedRouteTemplates;
    private IUrlParser urlParser;

    /// <summary>Constructor</summary>
    /// <param name="urlParser">Requires a parser that implements the interface IUrlParser</param>
    public EndpointMapper(IUrlParser urlParser)
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

    public Dictionary<HTTPMethod, List<string>> RegisteredEndpoints => this.parsedRouteTemplates;


    /// <summary>Register a route template pattern using HTTP method GET</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    public void RegisterEndpointGet(string routePattern)
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
    public void RegisterEndpointPost(string routePattern)
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
    public void RegisterEndpointPut(string routePattern)
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
    public void RegisterEndpointDelete(string routePattern)
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

    protected bool IsRouteAlreadyRegistered(string routePattern, HTTPMethod method)
    {
        return Enum.IsDefined<HTTPMethod>(method) && this.parsedRouteTemplates[method].Contains<string>(routePattern);
    }
    public void RegisterEndpoint(string routePattern, HTTPMethod method)
    {
        var parsedRoutePattern = this.urlParser.ReplaceTokensWithRegexPatterns(routePattern);

        if (Enum.IsDefined<HTTPMethod>(method) && !IsRouteAlreadyRegistered(parsedRoutePattern, method))
        {
            this.parsedRouteTemplates[method].Add(parsedRoutePattern);
        }
        else
        {
            throw new ArgumentException(
                $"Unkown HTTP Method provided. Acceptable methods are:" +
                "{HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
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
    public ResolvedUrl? TryMapRequestedRoute(string requestedUrl, HTTPMethod method)
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
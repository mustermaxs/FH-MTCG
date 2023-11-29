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
    private Dictionary<string, AbstractEndpoint> endpointMappings;
    private IUrlParser urlParser;

    /// <summary>Constructor</summary>
    /// <param name="urlParser">Requires a parser that implements the interface IUrlParser</param>
    public EndpointMapper(IUrlParser urlParser)
    {
        this.urlParser = urlParser;
        endpointMappings = new Dictionary<string, AbstractEndpoint>();
    }

    public Dictionary<string, AbstractEndpoint> RegisteredEndpoints => this.endpointMappings;

    // TODO use key as parsedRouteTempaltes key
    private string GetKeyForEndpointMapping(string url, HTTPMethod method)
    {
        return $"{method}-{url}";
    }

    /// <summary>Register a route template pattern using HTTP method GET</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    // public void RegisterEndpointGet(string routePattern)
    // {
    //     this.endpointMappings[HTTPMethod.GET].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    // }
    /// <summary>Register a route template pattern using HTTP method POST</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    // public void RegisterEndpointPost(string routePattern)
    // {
    //     this.endpointMappings[HTTPMethod.POST].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    // }
    /// <summary>Register a route template pattern using HTTP method PUT</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    // public void RegisterEndpointPut(string routePattern)
    // {
    //     this.endpointMappings[HTTPMethod.PUT].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    // }
    /// <summary>Register a route template pattern using HTTP method DELETE</summary>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>
    // public void RegisterEndpointDelete(string routePattern)
    // {
    //     this.endpointMappings[HTTPMethod.DELETE].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    // }

    /// <summary>Register a route template pattern.</summary>
    /// <param name="method">Expects the HTTP method for which the route is registered for.</param>
    /// <param name="routePattern">
    /// URL template for example like:
    /// - /api/without/named/params
    /// - /api/user/{userid:int} -> userid can only be an integer
    /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
    /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
    /// </param>

    protected bool IsRouteAlreadyRegistered(string route, HTTPMethod method)
    {
        string parsedRoutePattern = urlParser.ReplaceTokensWithRegexPatterns(route);
        string key = GetKeyForEndpointMapping(parsedRoutePattern, method);

        return endpointMappings.TryGetValue(key, out _);
    }
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, string controllerMethodName)
    {
        var parsedRoutePattern = this.urlParser.ReplaceTokensWithRegexPatterns(routePattern);

        if (!IsRouteAlreadyRegistered(parsedRoutePattern, method))
        {
            string key = GetKeyForEndpointMapping(routePattern, method);
            this.endpointMappings[key] = new Endpoint(method, parsedRoutePattern, controllerType, controllerMethodName);
        }
        else
        {
            throw new ArgumentException(
                $"Unkown HTTP Method provided. Acceptable methods are:" +
                "{HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }
    }

    private string RemovePrefixFromKey(string key)
    {
        string delimiter = "-";
        int delimiterIndex = key.IndexOf(delimiter);

        if (delimiterIndex != -1 && delimiterIndex < key.Length - 1)
        {
            throw new Exception($"Malformed key in EndpointMapper");
        }

        return key;
    }

    // TODO passenden Controller & Methode finden
    // wer soll das machen?
    // was soll Map eigentlich genau machen?
    // IMPROVE potentialPatterns umbenennen
    // ? was soll diese methode überhaupt machen
    /// <summary>Checks if the requested route is registered in the store.</summary>
    /// <returns>ResolvedUrl object containing (in case they exist) values of the named url parameters 
    /// and a bool (IsRouteRegistered) inidicating if the requested route was even registered in the store.</returns>
    public ResolvedUrl? TryMapRouteToEndpoint(string requestedUrl, HTTPMethod method)
    {
        string trimmedUrl = urlParser.CleanUrl(requestedUrl);
        Dictionary<string, string> namedTokensInUrlFound = new();
        var res = new ResolvedUrl(method, requestedUrl);



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
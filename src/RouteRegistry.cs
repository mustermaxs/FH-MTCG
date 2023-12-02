using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MTCG;

/// <summary>
/// Works as a store for registered routes. Can be used to 
/// check if requested route matches
/// any of the registered route patterns. 
/// If so, it can provide a ResolveRoute object containing the 
/// named parameters and their associated values
/// </summary>
/// TODO/10 IMPORTANT add Payload from HttpSvrEventArgs to Endpoint somehow
public class RouteRegistry : IEndpointMapper
{
    private Dictionary<HTTPMethod, List<IEndpoint>> endpointMappings;
    private IUrlParser parser;
    private static IEndpointMapper? _this = null;

    // TODO Dependency injection geht so nicht mehr
    public static IEndpointMapper GetInstance(IUrlParser urlParser)
    {
        if (_this == null)
        {
            _this = new RouteRegistry(urlParser);
        }
        return _this;
    }
    private RouteRegistry(IUrlParser urlParser)
    {
        this.parser = urlParser;
        this.endpointMappings = new Dictionary<HTTPMethod, List<IEndpoint>>
            {
                { HTTPMethod.POST, new List<IEndpoint>() },
                { HTTPMethod.GET, new List<IEndpoint>() },
                { HTTPMethod.PUT, new List<IEndpoint>() },
                { HTTPMethod.DELETE, new List<IEndpoint>() },
                { HTTPMethod.PATCH, new List<IEndpoint>() }
            };
    }
    public Dictionary<HTTPMethod, List<IEndpoint>> RegisteredEndpoints => this.endpointMappings;

    //TODO
    public bool IsRouteRegistered(string routeTemplate, HTTPMethod httpMethod)
    {
        List<IEndpoint> potentialEndpoints;

        if (endpointMappings.TryGetValue(httpMethod, out potentialEndpoints))
        {
            return (bool)(potentialEndpoints?.Any(endpoint => endpoint.RouteTemplate == routeTemplate));
        }

        return false;
    }
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, MethodInfo controllerMethod)
    {
        var parsedRoutePattern = this.parser.ReplaceTokensWithRegexPatterns(routePattern);

        if (!IsRouteRegistered(parsedRoutePattern, method))
        {
            this.endpointMappings[method].Add(new Endpoint(method, parsedRoutePattern, routePattern, controllerType, controllerMethod));
        }
        else
        {
            throw new ArgumentException(
                $"Unkown HTTP Method provided. Acceptable methods are:" +
                "{HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }
    }
    public void RegisterEndpoint(IEndpoint endpoint)
    {
        (string routeTemplate, HTTPMethod httpMethod, Type controllerType, MethodInfo controllerMethod) = endpoint;

        var parsedRoutePattern = this.parser.ReplaceTokensWithRegexPatterns(routeTemplate);

        if (!IsRouteRegistered(parsedRoutePattern, httpMethod))
        {
            this.endpointMappings[httpMethod].Add(new Endpoint(httpMethod, parsedRoutePattern, routeTemplate, controllerType, controllerMethod));
        }
        else
        {
            throw new ArgumentException(
                $"Unkown HTTP Method provided. Acceptable methods are:" +
                "{HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }
    }

    /// <summary>
    /// Checks if the requested route is registered in the store.
    /// </summary>
    /// <returns>
    /// ResolvedUrl object containing (in case they exist)
    /// values of the named url parameters 
    /// and a bool (IsRouteRegistered) inidicating 
    /// if the requested route was even registered in the store.
    /// </returns>
    public RouteContext MapRequestToEndpoint(string requestedUrl, HTTPMethod method)
    {
        List<IEndpoint> potentialEndpoints = endpointMappings[method];
        string trimmedUrl = parser.TrimUrl(requestedUrl);
        Dictionary<string, string> namedTokens = new();

        foreach (var endpoint in potentialEndpoints)
        {
            namedTokens = this.parser.MatchUrlAndGetParams(trimmedUrl, endpoint.EndpointPattern);

            if (namedTokens.Count > 0)
            {
                return new RouteContext(namedTokens, endpoint);
            }
        }

        return new TokenizedUrl();
    }

}
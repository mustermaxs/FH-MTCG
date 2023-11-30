using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG;

/// <summary>
/// Works as a store for registered routes. Can be used to 
/// check if requested route matches
/// any of the registered route patterns. 
/// If so, it can provide a ResolveRoute object containing the 
/// named parameters and their associated values
/// </summary>
public class EndpointMapper : IEndpointMapper
{
    private Dictionary<HTTPMethod, List<AbstractEndpoint>> endpointMappings;
    private IUrlParser parser;
    public EndpointMapper(IUrlParser urlParser)
    {
        this.parser = urlParser;
        this.endpointMappings = new Dictionary<HTTPMethod, List<AbstractEndpoint>>
            {
                { HTTPMethod.POST, new List<AbstractEndpoint>() },
                { HTTPMethod.GET, new List<AbstractEndpoint>() },
                { HTTPMethod.PUT, new List<AbstractEndpoint>() },
                { HTTPMethod.DELETE, new List<AbstractEndpoint>() },
                { HTTPMethod.PATCH, new List<AbstractEndpoint>() }
            };
    }
    public Dictionary<HTTPMethod, List<AbstractEndpoint>> RegisteredEndpoints => this.endpointMappings;

    //TODO
    public bool IsRouteRegistered(string routeTemplate, HTTPMethod method)
    {
        List<AbstractEndpoint> potentialEndpoints;

        if (endpointMappings.TryGetValue(method, out potentialEndpoints))
        {
            return (bool)(potentialEndpoints?.Any(endpoint => endpoint.RouteTemplate == routeTemplate));
        }

        return false;
    }
    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, string controllerMethodName)
    {
        var parsedRoutePattern = this.parser.ReplaceTokensWithRegexPatterns(routePattern);

        if (!IsRouteRegistered(parsedRoutePattern, method))
        {
            this.endpointMappings[method].Add(new Endpoint(method, parsedRoutePattern, routePattern, controllerType, controllerMethodName));
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
    public RequestObject? TryMapRouteToEndpoint(string requestedUrl, HTTPMethod method)
    {
        List<AbstractEndpoint> potentialEndpoints = endpointMappings[method];
        string trimmedUrl = parser.TrimUrl(requestedUrl);
        Dictionary<string, string> namedTokens = new();

        foreach (var endpoint in potentialEndpoints)
        {
            namedTokens = this.parser.MatchUrlAndGetParams(trimmedUrl, endpoint.EndpointPattern);

            if (namedTokens.Count > 0)
            {
                return new RequestObject(namedTokens, endpoint);
            }
        }

        return new RequestObject();
    }

}
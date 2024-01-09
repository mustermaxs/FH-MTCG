using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace MTCG;




/// <summary>
/// Works as a store for registered routes. Can be used to 
/// check if requested route matches
/// any of the registered route patterns. 
/// If so, it provides informations such as which controller and which
/// controller method to delegate the processing of the client request to.
/// </summary>

public class RouteRegistry : IEndpointMapper, IDisposable
{

    public void Dispose()
    {
        _this = null;
    }

    /// <summary>
    /// Stores a dictionary of all the registered endpoints in the application.
    /// Associates them with their HTTP-method for faster lookup and structure.
    /// </summary>

    private Dictionary<HTTPMethod, List<IEndpoint>> endpointMappings;
    private IUrlParser parser;



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Stores a static instance of a RouteRegistry.
    /// </summary>

    private static IEndpointMapper? _this = null;


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Return an instance of RouteRegistry class.
    /// Only one instance should exist since it registeres all the routes
    /// and should stay the same as long as the server is running.
    /// </summary>
    /// <param name="urlParser">
    /// Parser to extract (named) variables from the requested URL
    /// </param>

    public static IEndpointMapper GetInstance(IUrlParser urlParser)
    {
        if (_this == null)
        {
            _this = new RouteRegistry(urlParser);
        }
        return _this;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Constructor. Is private since only a single instance
    /// should be created.
    /// </summary>
    /// <param name="urlParser">
    /// Parser to extract (named) variables from the requested URL
    /// </param>

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

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public Dictionary<HTTPMethod, List<IEndpoint>> RegisteredEndpoints => this.endpointMappings;

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Checks if a route template is already registered.
    /// </summary>
    /// <param name="routeTemplate">
    /// Route template as defined via the route attributes
    /// </param>
    /// <param name="httpMethod">
    /// The HTTPMethod for which the route template in question is presumably
    /// registered (or not).
    /// </param>
    /// <returns>
    /// Boolean
    /// </returns>  

    public bool IsRouteRegistered(string routeTemplate, HTTPMethod httpMethod)
    {
        List<IEndpoint> potentialEndpoints;

        if (endpointMappings.TryGetValue(httpMethod, out potentialEndpoints))
        {
            return (bool)(potentialEndpoints?.Any(endpoint => endpoint.RouteTemplate == routeTemplate));
        }

        return false;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    /// <summary>
    /// Adds an endpoint to the registry.
    /// </summary>
    /// <param name="routePattern">
    /// Route template (e.g. IUrlParser.cs).
    /// </param>
    /// <param name="method">
    /// HTTPMethod for which the endpoint should be defined for.
    /// </param>
    /// <param name="controllerType">
    /// Type of the controller that's supposed to handle the request.
    /// </param>
    /// <param name="controllerMethod">
    /// Controller method that's supposed to handle the request.
    /// </param>

    public void RegisterEndpoint(string routePattern, HTTPMethod method, Type controllerType, MethodInfo controllerMethod, Role accesslevel)
    {
        var parsedRoutePattern = this.parser.ReplaceTokensWithRegexPatterns(routePattern);

        if (!IsRouteRegistered(parsedRoutePattern, method))
        {
            var endpointBuilder = new EndpointBuilder();

            endpointBuilder
                .WithHttpMethod(method)
                .WithControllerType(controllerType)
                .WithAccessLevel(accesslevel)
                .WithRoutePattern(routePattern)
                .WithControllerMethod(controllerMethod);


            this.endpointMappings[method].Add(endpointBuilder.Build());
        }
        else
        {
            throw new ArgumentException(
                $"Unkown HTTP Method provided. Acceptable methods are:" +
                "{HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Registeres an endpoint with a given IEndpoint object
    /// </summary>
    /// <param name="endpoint">
    /// Object implementing the IEndpoint interface
    /// </param>

    public void RegisterEndpoint(IEndpoint endpoint)
    {
        var parsedRoutePattern = this.parser.ReplaceTokensWithRegexPatterns(endpoint.RouteTemplate);

        if (!IsRouteRegistered(endpoint.RouteTemplate, endpoint.HttpMethod))
        {
            this.endpointMappings[endpoint.HttpMethod].Add(
                new Endpoint(
                    endpoint.HttpMethod,
                    parsedRoutePattern,
                    endpoint.RouteTemplate,
                    endpoint.ControllerType,
                    endpoint.ControllerMethod,
                    endpoint.AccessLevel));
        }
        else
        {
            throw new ArgumentException(
                $"Endpoint '{endpoint.RouteTemplate}' with HttpMethod '{endpoint.HttpMethod}' is already registered.");
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    /// <summary>
    /// Upon receiving a request for a specific endpoint, this method
    /// tries to map the requested URL and HTTPMethod to one of the registered
    /// endpoints.
    /// </summary>
    /// <param name="requestedUrl">
    /// The requested URL
    /// </param>
    /// <param name="method">
    /// The requested HTTP method
    /// </param>
    /// <returns>
    /// In case the requested endpoint can be found in the registry,
    /// this method returns an object that implements the IEndpoint interface.
    /// If it doesn't find a matching endpoint, it throws a RouteDoesntExistException.
    /// </returns>

    public IEndpoint? MapRequestToEndpoint(string requestedUrl, HTTPMethod method)
    {
        IEnumerable<IEndpoint> endpointsWithHttpMethod = endpointMappings[method];
        IEnumerable<IEndpoint> potentialEndpoints;
        string trimmedUrl = parser.TrimUrl(requestedUrl);
        Dictionary<string, string> urlParams = new();

        potentialEndpoints = endpointsWithHttpMethod.Where(
            e => parser.PatternMatches(trimmedUrl, e.EndpointPattern));

        if (potentialEndpoints.Count() == 0)
            throw new RouteDoesntExistException(requestedUrl);


        // check if exact match
        // else check if regex match is found
        // else, return null
        var endpoint =
            potentialEndpoints.FirstOrDefault<IEndpoint>(e => parser.TrimUrl(e.RouteTemplate) == trimmedUrl)
            ??
            potentialEndpoints.FirstOrDefault<IEndpoint>(e => parser.PatternMatches(trimmedUrl, e.EndpointPattern));


        if (endpoint == null)
            throw new RouteDoesntExistException(requestedUrl);

        return endpoint;
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    /// <summary>
    /// Upon receiving a request for a specific endpoint, this method
    /// tries to map the requested URL and HTTPMethod to one of the registered
    /// endpoints.
    /// </summary>
    /// <param name="ref context">
    /// Uses a reference to a RoutingContext object to gather all the
    /// necessary information concerning the clients request.
    /// </param>
    /// <returns>
    /// void. Stores all gathered information about the endpoint directly
    /// inside the RoutingContext object.
    /// Throws a RouteDoesntExistException in case the requested endpoint wasn't found.
    /// </returns>
    public void MapRequestToEndpoint(ref IRequest request)
    {
        var endpointsWithHttpMethod = endpointMappings[request.HttpMethod];
        string trimmedUrl = parser.TrimUrl(request.RawUrl!);
        IUrlParams urlParams;

        var potentialEndpoints = endpointsWithHttpMethod.Where(
            pe => parser.PatternMatches(trimmedUrl, pe.EndpointPattern)).ToList();

        if (potentialEndpoints.Count() == 0)
            throw new RouteDoesntExistException(request.RawUrl!);

        // check if exact match
        // else check if regex match is found
        // else, return null
        IEndpoint? endpoint =
            potentialEndpoints.FirstOrDefault<IEndpoint>(e1 => parser.TrimUrl(e1.RouteTemplate) == trimmedUrl)
            ?? potentialEndpoints.FirstOrDefault<IEndpoint>(e2 => parser.PatternMatches(trimmedUrl, e2.EndpointPattern));

        if (endpoint == null)
            throw new RouteDoesntExistException(request.RawUrl!);

        urlParams = this.parser.MatchUrlAndGetParams(trimmedUrl, endpoint!.EndpointPattern);
        endpoint.UrlParams = urlParams;
        request.Endpoint = (Endpoint)endpoint;
        request.RouteFound = true;
    }
}
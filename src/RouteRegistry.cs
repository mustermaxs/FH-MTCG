using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG;

public class ResolvedUrl
{
    private Dictionary<string, string> urlParams = new Dictionary<string, string>();
    private string? rawUrl;
    private HTTPMethod method;
    bool isRouteRegistered = false;
    public ResolvedUrl(Dictionary<string, string> parameters, HTTPMethod method, string rawUrl)
    {
        this.urlParams = parameters;
        this.method = method;
        this.rawUrl = rawUrl;

        if (urlParams.Count > 0)
        {
            isRouteRegistered = true;
        }
    }
    public bool IsRouteRegistered { get; private set; }

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

public class RouteRegistry
{
    public Dictionary<HTTPMethod, List<string>> routePatterns;
    private IUrlParser urlParser;
    public RouteRegistry(IUrlParser urlParser)
    {
        this.urlParser = urlParser;
        routePatterns = new Dictionary<HTTPMethod, List<string>>
        {
          { HTTPMethod.GET, new List<string>() },
          { HTTPMethod.POST, new List<string>() },
          { HTTPMethod.PUT, new List<string>() },
          { HTTPMethod.DELETE, new List<string>() },
          { HTTPMethod.PATCH, new List<string>() }
        };
    }

    public void RegisterGet(string routePattern)
    {
        this.routePatterns[HTTPMethod.GET].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }
    public void RegisterPost(string routePattern)
    {
        this.routePatterns[HTTPMethod.POST].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }
    public void RegisterPut(string routePattern)
    {
        this.routePatterns[HTTPMethod.PUT].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }
    public void RegisterDelete(string routePattern)
    {
        this.routePatterns[HTTPMethod.DELETE].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
    }

    public void RegisterRoute(string routePattern, HTTPMethod method)
    {
        if (Enum.IsDefined<HTTPMethod>(method))
        {
            this.routePatterns[method].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
        }
        else
        {
            throw new ArgumentException($"Unkown HTTP Method provided. Acceptable methods are: {HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }

    }

    public void RegisterRoute(string routePattern, HTTPMethod method, Type controllerType)
    {
        if (!Enum.IsDefined<HTTPMethod>(method))
        {
            throw new ArgumentException($"Unkown HTTP Method provided. Acceptable methods are: {HTTPMethod.GET}, {HTTPMethod.POST}, {HTTPMethod.PUT}, {HTTPMethod.DELETE}, {HTTPMethod.PATCH}.");
        }
        else if (!controllerType.IsAssignableTo(typeof(IController)))
        {
            throw new ArgumentException($"{controllerType} does not implement IController interface.");
        }
        else
        {
            this.routePatterns[method].Add(this.urlParser.ReplaceTokensWithRegexPatterns(routePattern));
        }
    }

    // TODO passenden Controller & Methode finden
    // wer soll das machen?
    // was soll Map eigentlich genau machen?
    // IMPROVE potentialPatterns umbenennen
    // ? was soll diese methode überhaupt machen
    public ResolvedUrl? MapRequest(string requestedUrl, HTTPMethod method)
    {
        Dictionary<string, string> namedTokensInUrlFound = new();
        List<string>? potentialPatterns;

        if (!routePatterns.TryGetValue(method, out potentialPatterns))
            return new ResolvedUrl(method, requestedUrl);

        string trimmedUrl = urlParser.CleanUrl(requestedUrl);


        foreach (string pattern in potentialPatterns)
        {
            namedTokensInUrlFound = this.urlParser.MatchUrlAndGetParams(trimmedUrl, pattern);

            if (namedTokensInUrlFound.Count > 0)
            {
                break;
            }
        }

        return new ResolvedUrl(namedTokensInUrlFound, method, requestedUrl);
    }

}
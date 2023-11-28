using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG;

class RouteRegistry
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
          { HTTPMethod.DELETE, new List<string>() }
        };
    }

    public void RegisterGet(string routePattern)
    {
        this.routePatterns[HTTPMethod.GET].Add(this.urlParser.CreateRegexPattern(routePattern));
    }
    public void RegisterPost(string routePattern)
    {
        this.routePatterns[HTTPMethod.POST].Add(this.urlParser.CreateRegexPattern(routePattern));
    }
    public void RegisterPut(string routePattern)
    {
        this.routePatterns[HTTPMethod.PUT].Add(this.urlParser.CreateRegexPattern(routePattern));
    }
    public void RegisterDelete(string routePattern)
    {
        this.routePatterns[HTTPMethod.DELETE].Add(this.urlParser.CreateRegexPattern(routePattern));
    }

    // TODO passenden Controller & Methode finden
    // wer soll das machen?
    // was soll Map eigentlich genau machen?
    // IMPROVE potentialPatterns umbenennen
    public GroupCollection Map(string requestedUrl, HTTPMethod method)
    {
        List<string> potentialPatterns;

        if (!routePatterns.TryGetValue(method, out potentialPatterns))
            return null;

        MatchCollection match = null;
        string requestedUrlCleaned = urlParser.CleanUrl(requestedUrl);
        

        foreach (string pattern in potentialPatterns)
        {
            match = Regex.MatchUrl(requestedUrlCleaned, pattern);

            if (match.Count > 0)
            {
                break;
            }
        }

        return match[0]?.Groups ?? null;

    }

}
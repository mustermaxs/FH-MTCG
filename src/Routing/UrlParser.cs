using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MTCG
{
    public class UrlParser : IUrlParser
    {
        public UrlParser() { }

        private string RemoveTrailingSlashes(string url)
        {
            return url.Trim('/');
        }



        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Removes trailing slaches from URL.
        /// </summary>
        public string TrimUrl(string url)
        {
            return RemoveTrailingSlashes(url);
        }



        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////




        /// <summary>
        /// Replaces the variable parameters in the URL template with regex patterns so that named
        /// tokens can be accessed by their name.
        /// </summary>
        /// <param name="urlTemplate">
        /// URL template for example like:
        /// - /api/without/named/params
        /// - /api/user/{userid:int} -> userid can only be an integer
        /// - /api/user/{username:alpha} -> username can only be letters from the alphabet
        /// - /api/token/{accesstoken:alphanum} -> accesstoken can be any string
        /// </param>
        /// <returns>The URL template with replaced regex patterns.</returns>
        public string ReplaceTokensWithRegexPatterns(string url)
        {
            string urlPattern = TrimUrl(url);
            // urlPattern = Regex.Escape(urlPattern);

            urlPattern = urlPattern.Replace("?", "\\?");
            urlPattern = Regex.Replace(urlPattern, @"(\{([a-zA-Z0-9-]+)(\:alpha)\})", @"(?<$2>[a-zA-Z-]+)");
            urlPattern = Regex.Replace(urlPattern, @"(\{([a-zA-Z0-9-]+)(\:alphanum)\})", @"(?<$2>[a-zA-Z0-9-]+)");
            urlPattern = Regex.Replace(urlPattern, @"(\{([a-zA-Z0-9-]+)(\:int)\})", @"(?<$2>[0-9]+)");
            urlPattern = "^" + urlPattern;

            return urlPattern;
        }



        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////



        protected Match? MatchAndIgnoreQueryString(string url, string pattern)
        {
            int queryStartPos = url.IndexOf("?");

            if (queryStartPos == -1) return Regex.Match(url, pattern + "$");

            var urlWithoutQueryString = url.Substring(0, queryStartPos);

            return Regex.Match(urlWithoutQueryString, pattern + "$");
        }




        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////




        /// <summary>Performs regex match to check if requested url matches provided pattern urlPattern</summary>
        /// <param name="urlPattern">Regex pattern (string)</param>
        /// <param name="url">Requested URL string. should be preprocessed with CleanUrl</param>
        /// <returns>Dictionary with param names as key and value as its value.</returns>
        public IUrlParams MatchUrlAndGetParams(string url, string urlPattern)
        {
            var queryParams = ExtractQueryParams(url, urlPattern);
            var namedParams = ExtractNamedParams(url, urlPattern);

            return new UrlParams(namedParams, queryParams);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        public Dictionary<string, string> ExtractQueryParams(string url, string urlPattern)
        {
            int queryStartPos = url.IndexOf("?");

            if (queryStartPos == -1)
                return new Dictionary<string, string>();

            var queryString = url.Substring(queryStartPos + 1);

            var keyValPairs = queryString.Split('&')
                .Select(pair => pair.Split('='))
                .ToDictionary(pair => pair[0], pair => pair[1]);

            return keyValPairs;
        }




        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////



        public Dictionary<string, string> ExtractNamedParams(string url, string urlPattern)
        {
            Match namedParamMatches = MatchAndIgnoreQueryString(url, urlPattern);
            Dictionary<string, string> namedGroups = new Dictionary<string, string>();

            if (!namedParamMatches.Success)
                return namedGroups;

            GroupCollection groups = namedParamMatches.Groups;

            for (int i = 1; i < groups.Count; i++)
            {
                string groupName = namedParamMatches.Groups[i].Name;
                string groupValue = namedParamMatches.Groups[i].Value;
                namedGroups[groupName] = groupValue;
            }

            return namedGroups;
        }



        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Checks if the provided URL matches
        /// a specific url pattern (regex).
        /// </summary>
        /// <param name="url">
        /// Raw url string.
        /// </param>
        /// <param name="urlPattern">
        /// Regex version of a url template.
        /// </param>
        /// <returns>
        /// Boolean
        /// </returns>
        public bool PatternMatches(string url, string urlPattern)
        {
            url = TrimUrl(url);

            return MatchAndIgnoreQueryString(url, urlPattern).Success;
            // return Regex.Match(url, urlPattern).Success;
        }
    }
}

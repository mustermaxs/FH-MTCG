using System.Collections.Generic;
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
            // urlPattern = Regex.Replace(urlPattern, @"\?[]")
            return urlPattern;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        /// <summary>Performs regex match to check if requested url matches provided pattern urlPattern</summary>
        /// <param name="urlPattern">Regex pattern (string)</param>
        /// <param name="url">Requested URL string. should be preprocessed with CleanUrl</param>
        /// <returns>Dictionary with param names as key and value as its value.</returns>
        public Dictionary<string, string> MatchUrlAndGetParams(string url, string urlPattern)
        {
            Match match = Regex.Match(url, urlPattern);

            if (!match.Success)
                return new Dictionary<string, string>();

            GroupCollection groups = match.Groups;
            Dictionary<string, string> namedGroups = new Dictionary<string, string>();

            for (int i = 1; i < groups.Count; i++)
            {
                string groupName = match.Groups[i].Name;
                string groupValue = match.Groups[i].Value;
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
            var prpdPatternForExactMatch = urlPattern.TrimStart('^').TrimEnd('$');

            return url == prpdPatternForExactMatch || Regex.Match(url, urlPattern).Success;
        }
    }
}

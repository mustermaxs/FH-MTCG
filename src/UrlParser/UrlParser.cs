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

        public string TrimUrl(string url)
        {
            return RemoveTrailingSlashes(url);
        }

        public string ReplaceTokensWithRegexPatterns(string url)
        {
            string urlPattern = TrimUrl(url);

            urlPattern = Regex.Replace(urlPattern, @"(\{([a-zA-Z0-9-]+)(\:alpha)\})", @"(?<$2>[a-zA-Z-]+)");
            urlPattern = Regex.Replace(urlPattern, @"(\{([a-zA-Z0-9-]+)(\:alphanum)\})", @"(?<$2>[a-zA-Z0-9-]+)");
            urlPattern = Regex.Replace(urlPattern, @"(\{([a-zA-Z0-9-]+)(\:int)\})", @"(?<$2>[0-9]+)");

            urlPattern = "^" + urlPattern + "$";

            return urlPattern;
        }
        public Dictionary<string, string> MatchUrlAndGetParams(string url, string urlPattern)
        {
            Match match = Regex.Match(url, urlPattern);

            if (!match.Success)
            {
                return new Dictionary<string, string>();
            }

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

        public bool PatternMatches(string url, string urlPattern)
        {
            return Regex.Match(url, urlPattern).Success;
        }
    }
}

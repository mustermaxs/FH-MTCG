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

            foreach (string groupName in groups.Keys)
            {
                namedGroups[groupName] = groups[groupName].Value;
            }

            return namedGroups;

        }
    }
}

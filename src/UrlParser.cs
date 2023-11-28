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

        public string CleanUrl(string url)
        {
            return RemoveTrailingSlashes(url);
        }

        public string CreateRegexPattern(string url)
        {
            string urlPattern = url;

            urlPattern = Regex.Replace(urlPattern, @"\/?(.*)(\{([a-zA-Z0-9-]+)(\:alpha)\})(.*)?", @"($1(?<$3>[a-zA-Z-]+)$5)");
            urlPattern = Regex.Replace(urlPattern, @"\/?(.*)(\{([a-zA-Z0-9-]+)(\:int)\})(.*)?", @"($1(?<$3>[0-9]+)$5)");

            urlPattern = "^" + urlPattern + "$";

            return urlPattern;
        }
        public Group MatchUrl(string url, string urlPattern)
        {
            Match match = Regex.Match(url, urlPattern);

            if (match.Count == 0)
            {
                return new Dictionary<stristrn
            }
                // Retrieve named groups
                GroupCollection groups = match.Groups;
                CaptureCollection captures = groups.Captures;

                // Create a dictionary to store named groups and their values
                Dictionary<string, string> namedGroups = new Dictionary<string, string>();

                // Iterate over named groups and add them to the dictionary
                foreach (string groupName in match.Groups.Names)
                {
                    namedGroups[groupName] = groups[groupName].Value;
                }

                return namedGroups;

            // Return an empty dictionary if there is no match
            return new Dictionary<string, string>();
        }
    }
}
}

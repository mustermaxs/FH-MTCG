using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG
{
    public interface IUrlParser
    {
        string CleanUrl(string url);
        string ReplaceTokensWithRegexPatterns(string urlTemplate);
        Dictionary<string, string> MatchUrlAndGetParams(string url, string urlPattern);
    }
}
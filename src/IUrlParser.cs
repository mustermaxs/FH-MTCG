using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG
{
    public interface IUrlParser
    {
        string CleanUrl(string url);
        string CreateRegexPattern(string urlTemplate);
        Group MatchUrl(string url, string urlPattern);
    }
}
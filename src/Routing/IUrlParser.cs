using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG
{
    /// <summary>Interface for URL parsers.</summary>
    public interface IUrlParser
    {


        /// <summary>Perform trimming and other necessary preparations of the raw url string for further parsing</summary>
        /// <returns>Prepared url string for further processing/parsing</returns>
        string TrimUrl(string url);




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
        string ReplaceTokensWithRegexPatterns(string urlTemplate);
        



        /// <summary>Performs regex match to check if requested url matches provided pattern urlPattern</summary>
        /// <param name="urlPattern">Regex pattern (string)</param>
        /// <param name="url">Requested URL string. should be preprocessed with CleanUrl</param>
        /// <returns>Returns IUrlparams</returns>
        IUrlParams MatchUrlAndGetParams(string url, string urlPattern);


        
        public bool PatternMatches(string url, string urlPattern);
    }
}
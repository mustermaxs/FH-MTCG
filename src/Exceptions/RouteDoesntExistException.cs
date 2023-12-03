using System;

namespace MTCG
{
    public class RouteDoesntExistException : Exception
    {
        private string invalidUrl;

        public string Url => invalidUrl;

        public RouteDoesntExistException(string invalidUrl)
        {
            this.invalidUrl = invalidUrl;
        }

        public RouteDoesntExistException(string invalidUrl, string message)
            : base(message)
        {
            this.invalidUrl = invalidUrl;
        }

        public RouteDoesntExistException(string invalidUrl, string message, Exception inner)
            : base(message, inner)
        {
            this.invalidUrl = invalidUrl;
        }
    }
}

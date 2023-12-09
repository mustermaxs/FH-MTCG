using System;
using MTCG;


namespace MTCG
{
    public enum ACCESS
    {
        USER,
        ADMIN,
        ALL,
        NOT_LOGGEDIN
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        private string routeTemplate;
        private HTTPMethod method;
        private ACCESS accessLevel;

        public RouteAttribute(string route, HTTPMethod method, ACCESS accessLevel)
        {
            this.routeTemplate = route;
            this.method = method;
            this.accessLevel = accessLevel;
        }

        public string RouteTemplate
        {
            get { return routeTemplate; }
        }

        public HTTPMethod Method => method;
        public ACCESS AccessLevel => accessLevel;
    }
}
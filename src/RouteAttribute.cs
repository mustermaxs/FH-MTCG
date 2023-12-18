using System;
using MTCG;


namespace MTCG
{
    [Flags]
    public enum Role
    {
        ANONYMOUS = 1,
        USER = 2,
        ADMIN = 4,
        ALL = ANONYMOUS | USER | ADMIN
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        private string routeTemplate;
        private HTTPMethod method;
        private Role accessLevel;

        public RouteAttribute(string route, HTTPMethod method, Role accessLevel)
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
        public Role AccessLevel => accessLevel;
    }
}
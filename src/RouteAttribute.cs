using System;
using MTCG;


namespace MTCG
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        private string routeTemplate;
        private HTTPMethod? method;

        public RouteAttribute(string route, HTTPMethod method)
        {
            this.routeTemplate = route;
            this.method = method; // Add this line to assign the value of method
        }

        public string RouteTemplate
        {
            get { return routeTemplate; }
        }

        public HTTPMethod Method
        {
            get { return method; }
        }
    }
}
using System;
using MTCG;


namespace MTCG
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RouteAttribute : Attribute
{
    private string route;
    private HTTPMethod method;

    public RouteAttribute(string route, HTTPMethod method)
    {
        this.route = route;
        this.method = method; // Add this line to assign the value of method
    }

    public string Route
    {
        get { return route; }
    }

    public HTTPMethod Method
    {
        get { return method; }
    }
}
}
using System;
using MTCG;


namespace MTCG;



/// <summary>
/// Client roles.
/// </summary>
[Flags]
public enum Role
{
    ANONYMOUS = 1, // for clients who are not logged in
    USER = 2,
    ADMIN = 4,
    ALL = ANONYMOUS | USER | ADMIN
}


/// <summary>
/// Marks a (controller) method as the handler for a specific endpoint.
/// </summary>
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
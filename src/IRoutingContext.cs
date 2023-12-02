using System;

namespace MTCG;

public abstract class IRountingContext
{
    protected string? rawUrl;

    protected HTTPMethod method;

    protected bool routeFound = false;

    public bool RouteFound => routeFound;

    public HTTPMethod Method => this.method;

    public string? RawUrl => this.rawUrl;

}
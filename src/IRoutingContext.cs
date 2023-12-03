using System;

namespace MTCG;

public abstract class IRoutingContext
{
  protected string? rawUrl;

  public HTTPMethod httpMethod { get; protected set; }

    protected bool routeFound = false;

    public bool RouteFound
    {
        get => routeFound;
        set { routeFound = value; }
    }

    public HTTPMethod Method => this.httpMethod;

    /// 12.02.2023 21:13
    /// FIXME default value
    public virtual HttpHeader[] Headers { protected get; set; } = new HttpHeader[0];


    public IEndpoint? Endpoint
    {
        get; set;
    }
  public string? RawUrl { get => rawUrl; }
}
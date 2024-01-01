using System;


namespace MTCG;

/// <summary>
/// Represents a builder class for constructing HTTP requests.
/// </summary>
public class RequestBuilder
{
  private HTTPMethod httpMethod;
  private HttpHeader[] httpHeaders;
  private string payload;
  private string route;
  private string sessionId;

  public RequestBuilder() {}

    public RequestBuilder WithHttpMethod(HTTPMethod method)
    {
        this.httpMethod = method;

        return this;
    }

    public RequestBuilder WithHeaders(HttpHeader[] headers)
    {
        this.httpHeaders = headers;

        return this;
    }

    public RequestBuilder WithSessionId(string sessionId)
    {
        this.sessionId = sessionId;

        return this;
    }

    public RequestBuilder WithPayload(string payload)
    {
        this.payload = payload;

        return this;
    }

    public RequestBuilder WithRoute(string requestedRoute)
    {
        this.route = requestedRoute;

        return this;

    }

    public Request Build()
    {
        return new Request(httpMethod, route, httpHeaders, payload, sessionId);
    }
}
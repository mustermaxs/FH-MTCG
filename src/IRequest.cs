using System;

namespace MTCG;

/// 12.03.2023 17:25
/// TODO
public interface IRequest
{
    Dictionary<string, string> UrlParams { get; set; }
    bool TryGetHeader(string key, out string value);
    T GetParam<T>(string key);
    bool SetParam(string key, string value);

    HTTPMethod HttpMethod { get; }
    HttpHeader[] Headers { get; set; }
    IEndpoint? Endpoint { get; set; }
    string? RawUrl { get; }
    string? Payload { get; set; }
    bool RouteFound { get; set; }
    string? SessionId { get; }
    public T? PayloadAsObject<T>() where T : class;
    public IEnumerable<T>? PayloadAsEnumerable<T>() where T : class;
    public List<T>? PayloadAsList<T>() where T : class;

}

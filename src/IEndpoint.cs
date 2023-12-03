using System;
using System.Data.SqlTypes;
using System.Reflection;

namespace MTCG;

/// 12.03.2023 13:48
/// REFACTOR überlegen welche fields vlt besser in der
/// Endpoint implementierung sein sollten
public interface IEndpoint
{
    public Dictionary<string, string> UrlParams { get; set; }
    public string RouteTemplate { get; }
    public HTTPMethod HttpMethod { get; set; }
    public string EndpointPattern { get; }
    Type ControllerType { get; }
    MethodInfo ControllerMethod { get; }
}
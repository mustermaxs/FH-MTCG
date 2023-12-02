using System;

namespace MTCG;

/// 12.02.2023 02:11
/// TODO
public interface IRouter
{
    public void RegisterRoutes();
    /// 12.02.2023 02:24
    /// ? um request object zu instantiieren
    public IRequest ObtainClientRequest();
    public void HandleRequest(object sender, HttpSvrEventArgs e);
    /// 12.02.2023 02:11
    /// ? brauch ich das
    public void HandleResponse();
}
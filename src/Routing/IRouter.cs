using System;
using System.Threading.Tasks;

namespace MTCG;

/// 12.02.2023 02:11
/// TODO
/// TODO in Router-Implementierung Methoden um Controller zu instanziieren
public interface IRouter
{
    void RegisterRoutes();
    /// 12.02.2023 02:24
    /// ? um request object zu instantiieren
    // public IRequest ObtainClientRequest();
    Task<IResponse> HandleRequest(IRequest request);
    /// 12.02.2023 02:11
    /// ? brauch ich das
    // public void HandleResponse();
}

using System;

namespace MTCG;

public interface ITokenValidator
{
    public bool ValidateToken(string token);
    public bool TokenIsValid(string token);
    public User GetClientModel();
}

public abstract class Session
{

}

public abstract class BaseSessionManager
{
    ///? wie session id generieren
    /// session id in request object speichern,
    /// sessions field **static** speichern
    ///? oder in request session speichern?
    public BaseSessionManager()
    {
        
    }
    
    private static Dictionary<string, Session> sessions;
    abstract Session CreateSession(IRequest request);
    virtual public Session GetSessionById(string sessionId);



}
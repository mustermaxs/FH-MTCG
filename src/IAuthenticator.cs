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

    protected static Dictionary<string, Session> sessions;
    public abstract Session CreateSession(IRequest request);
    abstract public Session GetSessionById(string sessionId);
}
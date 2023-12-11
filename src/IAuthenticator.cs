using System;

namespace MTCG;

public interface ITokenHandler
{
    public bool ValidateToken(string token);
    public bool TokenIsValid(string token);
    public User GetClientModel();
}

public class Session
{
    public Session(string sessionId, User client)
    {
        Id = sessionId;
        this.User = client;
    }
    public readonly string Id;
    public readonly User User;

}

// public abstract class BaseSessionManager
// {

//     /// CREATE SESSION
//     ///     client logs in
//     ///         + SessionController
//     ///         + SessionRespository
//     ///             check token
//     ///             if valid
//     ///                 return token
//     ///             else
//     ///                 401 Error
//     ///          
//     ///         + create session with user model instance
//     ///         + store sessionID in request object
//     ///         + store session in SessionManager
//     ///
//     ///     client already logged in
//     ///         + TryGetSession
//     ///         if Session found


//     ///? wie session id generieren
//     /// session id in request object speichern,
//     /// sessions field **static** speichern
//     ///? oder in request session speichern?
//     // public BaseSessionManager(ITokenValidator tokenValidator)
//     // {
//     //     this.tokenValidator = tokenValidator;
//     // }

//     // protected ITokenValidator tokenValidator;
//     static BaseSessionManager()
//     {
//         BaseSessionManager.sessions = new Dictionary<string, Session>();
//     }
//     private static Dictionary<string, Session> sessions;
//     protected static Dictionary<string, Session> Sessions { get => sessions; }
//     public abstract string CreateSession(string sessionId, User client);
//     public abstract void EndSession(string sessionId);
//     abstract public bool TryGetSession(string sessionId, out Session session);
// }


public abstract class BaseSessionManager
{
    private static Dictionary<string, Session> sessions = new Dictionary<string, Session>();

    protected static Dictionary<string, Session> Sessions => sessions;
}
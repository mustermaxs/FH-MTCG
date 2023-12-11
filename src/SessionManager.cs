using System;
using Npgsql.Replication;

namespace MTCG;




/// <summary>
/// Stores each created session in a static
/// dictionary.
/// The inidivual sessions provide client
/// details (if logged in) among other things.
/// </summary>
public class SessionManager : BaseSessionManager
{



    /// <summary>
    /// Creates a new session object and stores it
    /// in a static dictionary.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static string CreateSession(string sessionId, User client)
    {
        var session = new Session(sessionId, client);
        Sessions.Add(sessionId, session);
        
        return sessionId;
    }




    public static void EndSession(string sessionId)
    {
        Sessions.Remove(sessionId);
    }




    public static bool TryGetSession(string sessionId, out Session session)
    {
        session = null;
        if (Sessions.TryGetValue(sessionId, out Session retrievedSession))
        {
            session = retrievedSession;
            return true;
        }
        return false;
    }
}
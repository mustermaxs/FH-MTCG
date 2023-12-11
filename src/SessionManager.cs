using System;
using Npgsql.Replication;

namespace MTCG;

public class SessionManager : BaseSessionManager
{
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
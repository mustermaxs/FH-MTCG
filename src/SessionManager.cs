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


    private static object _sessionLock = new object();

    /// <summary>
    /// Creates a new session object and stores it
    /// in a static dictionary.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="client"></param>
    /// <returns>
    /// string - session id that can be used to access session
    /// in static session dictionary.
    /// </returns>
    public static string CreateSession(string authToken, User user)
    {
        string sessionId = SessionManager.CreateSessionIdFromAuthToken(authToken);
        Session session = new Session(sessionId).WithAuthToken(authToken).WithUser(user);

        try
        {
            Sessions.TryAdd(sessionId, session);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating session: {e.Message}. Already exists.");
        }

        return sessionId;
    }

    public static void EndSession(string authToken)
    {
        var sessionId = SessionManager.CreateSessionIdFromAuthToken(authToken);

        lock (_sessionLock)
        {
            Sessions.Remove(sessionId, out _);
        }
    }

    public static bool TryGetSession(string unencryptedSessionId, out Session session)
    {
        string sessionId = CryptoHandler.Encode(unencryptedSessionId);

        lock (_sessionLock)
        {
            Session retrievedSession;

            if (Sessions.TryGetValue(sessionId, out retrievedSession))
            {
                session = retrievedSession;

                return true;
            }

            session = null!;

            return false;
        }
    }
}
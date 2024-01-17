using System;
using Npgsql.Replication;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


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
    /// Creates a new session object for a registered user and stores it
    /// in a static dictionary.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="client"></param>
    /// <returns>
    /// string - session id that can be used to access session
    /// in static session dictionary.
    /// </returns>
    public static bool TryCreateSessionForUser(string authToken, User user, out Session session)
    {
        lock (_sessionLock)
        {
            string sessionId = SessionManager.CreateSessionIdFromAuthToken(authToken);
            Session newSession = new Session(sessionId).WithAuthToken(authToken).WithUser(user);
            newSession.IsAnonymous = false;
            user.Token = authToken;
            try
            {
                Sessions.TryAdd(sessionId, newSession);

                session = newSession;

                return true;
            }
            catch (Exception e)
            {
                Logger.Err($"creating session: {e.Message}. Already exists.");
                throw e;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static bool IsLoggedIn(string token)
    {
        return Sessions.ContainsKey(CreateSessionIdFromAuthToken(token));
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static bool UpdateSession(string sessionId, ref Session? session)
    {
        lock (_sessionLock)
        {
            if (!Sessions.TryGetValue(sessionId, out Session? existingSession))
                return false;

            session = existingSession;

            return true;
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public static void UpdateUser(User user)
    {
        lock (_sessionLock)
        {
            Session? session = null;
            var id = CreateSessionIdFromAuthToken(user.Token);
            if (Sessions.TryGetValue(id, out session))
            {
                session.User = user;
                session.IsAnonymous = false;
                Sessions[id] = session;
            }
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static string CreateAnonymSessionReturnId()
    {
        var randId = CreateAuthToken(new Random().Next(int.MaxValue).ToString());
        string sessionId = SessionManager.CreateAuthToken(randId);
        Session session = new Session(sessionId).WithUser(new User());
        session.IsAnonymous = true;

        try
        {
            Sessions.TryAdd(sessionId, session);
        }
        catch (Exception e)
        {
            Logger.Err($"creating session: {e.Message}. Already exists.");
        }
        return session.SessionId;
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    // TODO return true on success
    public static void EndSession(string authToken)
    {
        var sessionId = SessionManager.CreateSessionIdFromAuthToken(authToken);

        lock (_sessionLock)
        {
            Sessions.Remove(sessionId, out _);
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static bool EndSessionWithSessionId(string sessionId)
    {
        bool removedSession = false;

        lock (_sessionLock)
        {
            removedSession = Sessions.Remove(sessionId, out _);
        }

        return removedSession;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static bool TryGetSessionWithToken(string authToken, out Session session)
    {
        string sessionId = CryptoHandler.Encode(authToken);

        lock (_sessionLock)
        {
            Session? retrievedSession;

            if (Sessions.TryGetValue(sessionId, out retrievedSession))
            {
                session = retrievedSession;

                return true;
            }

            session = null!;

            return false;
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static Session? GetSessionById(string sessionId)
    {
        lock (_sessionLock)
        {
            Session? retrievedSession = null;

            Sessions.TryGetValue(sessionId, out retrievedSession);

            return retrievedSession;
        }
    }


    public static bool TryGetSessionById(string sessionId, out Session session)
    {
        lock (_sessionLock)
        {
            Session? retrievedSession;

            if (Sessions.TryGetValue(sessionId, out retrievedSession))
            {
                session = retrievedSession;

                return true;
            }

            session = null!;

            return false;
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public static User? GetUserBySessionId(string sessionId)
    {
        lock (_sessionLock)
        {
            Session? retrievedSession = null;

            Sessions.TryGetValue(sessionId, out retrievedSession);

            return retrievedSession?.User;
        }
    }

}
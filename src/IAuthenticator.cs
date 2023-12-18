using System;

namespace MTCG;

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
    private const int LENGTH_AUTH_TOKEN = 10;
    static protected IRepository<User>? userRepository;

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    ///
    protected static string CreateSessionIdFromAuthToken(string authToken)
    {
        return CryptoHandler.Encode(authToken);
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Generates a random authentication token.
    /// </summary>
    /// <returns>A string representing the authentication token.</returns>
    public static string CreateAuthToken()
    {
        Random random = new Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[LENGTH_AUTH_TOKEN];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new String(stringChars);
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Generates a random string with the specified length.
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <returns>A random string with the specified length.</returns>
    public static string GetRandomStringWithLength(int length)
    {
        Random random = new Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[length];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new String(stringChars);
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Creates an authentication token based on the provided seed.
    /// </summary>
    /// <param name="seed">The seed used to generate the authentication token.</param>
    /// <returns>The generated authentication token.</returns>
    public static string CreateAuthToken(string seed)
    {
        return CryptoHandler.Encode(seed);
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sets the repository to be used for user authentication.
    /// </summary>
    /// <param name="repo">The repository to be used.</param>
    public static void UseRepo(IRepository<User> repo) { BaseSessionManager.userRepository = repo; }

    private static Dictionary<string, Session> sessions = new Dictionary<string, Session>();

    protected static Dictionary<string, Session> Sessions => sessions;
}
using System;
using System.Collections.Concurrent;

namespace MTCG;

// TODO unnötig, gleich in class SessionManager implementieren
// TODO could use the auth token directory as key -> no need for
// creating this random string
public abstract class BaseSessionManager
{
    private const int LENGTH_AUTH_TOKEN = 10;
    static protected IRepository<User>? userRepository;

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    ///
    protected static string CreateSessionIdFromAuthToken(string authToken)
    {
        return Encoder.Encode(authToken);
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
        return Encoder.Encode(seed);
    }

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sets the repository to be used for user authentication.
    /// </summary>
    /// <param name="repo">The repository to be used.</param>
    public static void UseRepo(IRepository<User> repo) { BaseSessionManager.userRepository = repo; }

    private static ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();

    protected static ConcurrentDictionary<string, Session> Sessions => sessions;
}
namespace MTCG;

public class Session : IModel
{

    public Session() { }
    public Session(string sessionId)
    {
        SessionId = sessionId;
    }

    public string SessionId { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;
    public User? User { get; private set; } = null;
    public bool IsLoggedIn { get => User != null && AuthToken != ""; }

    public Session WithAuthToken(string authToken)
    {
        if (AuthToken != string.Empty)
        {
            throw new Exception("AuthToken already set");
        }
        
        AuthToken = authToken;

        return this;
    }

    public Session WithSessionId(string sessionId)
    {
        if (SessionId != string.Empty)
        {
            throw new Exception("SessionId already set");
        }

        SessionId = sessionId;

        return this;
    }

    public Session WithUser(User user)
    {
        if (User != null)
        {
            throw new Exception("User already set");
        }

        User = user;

        return this;
    }
}

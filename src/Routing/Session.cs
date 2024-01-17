namespace MTCG;


/// <summary>
/// 
/// </summary>
public class Session
{
    public Session()
    {
    }
    public Session(string sessionId)
    {
        SessionId = sessionId;
    }
    public bool IsAnonymous { get; set; } = false;
    public string SessionId { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;
    public User? User { get; set; } = null; // setter used to be private, updating 
                                            // is easier like this
    public bool IsLoggedIn { get => User != null && AuthToken != ""; }
    public ResponseTextTranslator responseTxt { get; protected set; } = new ResponseTextTranslator();
    public string Language
    {
        get {
            return Language;

        }
        set
        {
            if (value != string.Empty)
            {
                Language = value;
                responseTxt.SetLanguage(value);
            }
            else
            {
                Language = "english";
            }
        }
    } 


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
        responseTxt.SetLanguage(User.Language);
        responseTxt = responseTxt.Load<ResponseTextTranslator>();

        return this;
    }
}
using System.Runtime.CompilerServices;
using System.Text.Json;
using MTCG;
using Npgsql;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


/// <summary>
/// Controller for user related things.
/// </summary>
[Controller]
public class UserController : BaseController
{

    protected static UserRepository repo = ServiceProvider.GetFreshInstance<UserRepository>();
    public UserController(IRequest request) : base(request) { }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/session/", HTTPMethod.POST, Role.ANONYMOUS)]
    public IResponse Login()
    {
        try
        {
            User? payload = request.PayloadAsObject<User>();

            if (payload == null) return new Response<string>(401, resConfig["USR_CRD_INVALID"]);

            string username = payload.Name;
            string password = payload.Password;

            if (!TryValidateCredentials(username, password, out User? user))
                return new Response<string>(401, resConfig["USR_CRD_INVALID"]);

            string authToken = SessionManager.CreateAuthToken(user!.ID.ToString());

            if (!SessionManager.TryCreateSessionForUser(authToken, user, out Session session))
                return new Response<string>(500, resConfig["INT_SVR_ERR"], "Failedd to create session.");

            InitUserSettings(session, user);

            return new Response<object>(200, new { authToken }, resConfig["USR_LOGIN_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to login user. {ex.Message}");

            return new Response<string>(500, resConfig["USR_LOGIN_ERR"]);
        }
    }

    /// <summary>
    /// Initializes the user settings for the given session.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="user"></param>
    /// wäre wsl besser das irgendwoanders zu machen
    protected void InitUserSettings(Session session, User user)
    {
        SessionManager.UpdateSession(session.AuthToken, ref session!);
        session.User.Language = user.Language;
        resConfig = ServiceProvider.Get<ResponseTextTranslator>();
        resConfig.SetLanguage(user.Language);
    }

    /// <summary>
    /// Checks if the given username and password are valid.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private bool TryValidateCredentials(string username, string password, out User? user)
    {
        user = repo.GetByName(username);
        var hashedPwd = Encoder.Encode(password);

        if (user == null || hashedPwd != user.Password)
            return false;

        return true;
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/session/logout", HTTPMethod.POST, Role.USER | Role.ADMIN)]
    public IResponse Logout()
    {
        try
        {
            bool endedSession = false;

            endedSession = SessionManager.EndSessionWithSessionId(request.SessionId);

            if (!endedSession) return new Response<string>(403, resConfig["LOGOUT_ERR"]);

            return new Response<string>(200, resConfig["LOGOUT_SUCC"]);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to logout user.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }










    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/users/", HTTPMethod.POST, Role.ANONYMOUS)]
    public IResponse RegisterNewUser()
    {
        try
        {
            var user = JsonSerializer.Deserialize<User>(request.Payload);
            Logger.ToConsole($"USER: {user!.Name}");
            if (user == null)
                return new Response<string>(500, resConfig["USR_ADD_NO_USER"]);

            user.Password = Encoder.Encode(user.Password);

            repo.Save(user);

            return new Response<string>(201, resConfig["USR_ADD_SUCC"]);
        }
        catch (PostgresException pex) // in case registration failed due to user already existing or other reasons
        {
            ErrorResponse<User> errResponse = new(resConfig["USR_ADD_EXISTS_ERR"], 500);

            if (pex.SqlState == "23505") errResponse = new(resConfig["USR_ADD_EXISTS_ERR"], 500);
            else errResponse = new(resConfig["USR_ADD_ERR"]);

            Console.WriteLine($"{errResponse.Description}. {pex}");

            return errResponse;
        }
        catch (Exception ex)
        {
            Logger.Err(ex, true);

            return new ErrorResponse<User>($"{resConfig["USR_ADD_ERR"]} {ex}", 500);
        }
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    [Route("/users/{username:alphanum}", HTTPMethod.PUT, Role.USER | Role.ADMIN)]
    public IResponse UpdateUser(string username)
    {
        try
        {
            var user = SessionManager.GetUserBySessionId(request.SessionId);
            var updatedUser = request.PayloadAsObject<User>();
            var userInDb = repo.GetByName(username);

            if (userInDb == null)
                return new Response<string>(404, resConfig["USR_NOT_FOUND"]);


            if (user.Name == userInDb.Name && user.Bio == userInDb.Bio && user.Name == username)
            {
                user.Bio = updatedUser.Bio;
                user.Image = updatedUser.Image;
                user.Name = updatedUser.Name;

                repo.Update(user);

                return new Response<string>(200, resConfig["USR_UPDATE_SUCC"]);
            }
            else
            {
                return new Response<string>(404, resConfig["USR_NOT_FOUND"]);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update user.\n{ex}");

            return new Response<string>(500, resConfig["INT_SVR_ERR"]);
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/users/", HTTPMethod.GET, Role.ADMIN)]
    public IResponse GetAllUsers()
    {
        try
        {
            IEnumerable<User> users = repo.GetAll();

            return new SuccessResponse<IEnumerable<User>>(200, users, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get all users. {ex.Message}");

            return new ErrorResponse<User>($"Failed to get all users.");
        }
    }


    [Route("/users/{id:alphanum}", HTTPMethod.DELETE, Role.ADMIN)]
    public IResponse DeleteUser(Guid id)
    {
        try
        {
            var user = repo.Get(id);

            if (user == null)
                return new Response<string>(404, resConfig["USR_NOT_FOUND"]);

            repo.Delete(user);

            return new Response<string>(200, resConfig["USR_DELETE_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete user. {ex.Message}");

            return new Response<string>(500, "Failed to delete user");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/users/id/{userid:alphanum}", HTTPMethod.GET, Role.ADMIN)]
    public IResponse GetUserById(Guid userid)
    {
        User? user = repo.Get(userid);

        if (user == null)
            return new Response<string>(404, "User not found.");

        return new Response<User>(200, user, "");
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    [Route("/users/{username:alpha}", HTTPMethod.GET, Role.USER | Role.ADMIN)]
    public IResponse GetUserByName(string username)
    {
        try
        {
            if (LoggedInUser.UserAccessLevel == Role.USER)
            {
                if (LoggedInUser.Name != username) return new Response<string>(404, resConfig["AUTH_ERR"]);
            }

            User? user = repo.GetByName(username);

            if (user == null)
                return new Response<string>(404, "User not found.");

            return new Response<User>(200, user, "Data successfully retrieved");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get user by name.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    [Route("/users/settings/language", HTTPMethod.POST, Role.ADMIN | Role.USER | Role.ALL)]
    public IResponse ChangeResponseLanguage()
    {
        var payload = request.PayloadAsObject<Dictionary<string, string>>();

        if (payload.TryGetValue("language", out string language))
        {
            if (resConfig.Response.ContainsKey(language))
            {
                return new Response<string>(200, "Language successfully changed.");
            }

        }
        return new Response<string>(400, "Bad request.");
    }
}
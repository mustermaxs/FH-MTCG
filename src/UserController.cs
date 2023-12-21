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
public class UserController : IController
{

    protected static UserRepository repo = new UserRepository();
    public UserController(IRequest request) : base(request) { }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/session/", HTTPMethod.POST, Role.ANONYMOUS)]
    public IResponse Login()
    {
        try
        {
            User? payload = request.PayloadAsObject<User>();

            string username = payload?.Name;
            string password = payload?.Password;

            User? user = repo.GetByName(username);
            var hashedPwd = CryptoHandler.Encode(password);

            if (user == null || hashedPwd != user.Password)
                return new Response<User>(401, "Invalid username/password provided");

            string authToken = SessionManager.CreateAuthToken(user.ID.ToString());
            SessionManager.CreateSessionForUser(authToken, user);

            return new SuccessResponse<object>(200, new { authToken }, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to login user. {ex.Message}");

            return new ErrorResponse<User>($"Failed to login user.");
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

            if (user == null)
                return new ErrorResponse<User>($"No or wrong user provided", 500);

            user.Password = CryptoHandler.Encode(user.Password);

            repo.Save(user);

            return new ResponseWithoutPayload(201, "User successfully created.");
        }
        catch (PostgresException pex) // in case registration failed due to user already existing or other reasons
        {
            ErrorResponse<User> errResponse = new("", 500);

            if (pex.SqlState == "23505") errResponse = new($"User with same username already registered", 500);
            else errResponse = new($"Failed to register new user.");

            Console.WriteLine($"{errResponse.Description}. {pex}");

            return errResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new ErrorResponse<User>($"Failed to register new user. {ex}", 500);
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

            if (user.Name == userInDb.Name && user.Bio == userInDb.Bio && user.Name == username)
            {
                user.Bio = updatedUser.Bio;
                user.Image = updatedUser.Image;
                user.Name = updatedUser.Name;

                repo.Update(user);

                return new Response<string>(200, "User sucessfully updated.");
            }
            else
            {
                return new Response<string>(404, "User not found.");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update user.\n{ex}");

            return new Response<string>(500, "Something went wrong :(");
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/users/", HTTPMethod.GET, Role.ALL)]
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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    [Route("/users/{userid:alphanum}", HTTPMethod.GET, Role.ALL)]
    public IResponse GetUserById(Guid userid)
    {
        User? user = repo.Get(userid);
        Thread.Sleep(2000);

        return new SuccessResponse<User>(200, user, "");

    }
}
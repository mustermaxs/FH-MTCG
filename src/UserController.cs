using System.Runtime.CompilerServices;
using System.Text.Json;
using MTCG;
using Npgsql;

namespace MTCG
{
    [Controller]
    public class UserController : IController
    {

        protected static IRepository<User> repo = new UserRepository();

        public UserController(IRequest request) : base(request) { }



        [Route("/users/", HTTPMethod.POST, ACCESS.NOT_LOGGEDIN)]
        public IResponse RegisterNewUser()
        {
            try
            {
                var user = JsonSerializer.Deserialize<User>(request.Payload);

                repo.Save(user);

                return new ResponseWithoutPayload(201, "User successfully created.");
            }
            catch (PostgresException pex)
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



        [Route("/users/", HTTPMethod.GET, ACCESS.ALL)]
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

        [Route("/users/{userid:alphanum}", HTTPMethod.GET, ACCESS.ALL)]
        public IResponse GetUserById(Guid userid)
        {
            User? user = repo.Get(userid);
            Thread.Sleep(2000);

            return new SuccessResponse<User>(200, user, "");

        }
    }
}

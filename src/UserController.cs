using System.Runtime.CompilerServices;
using System.Text.Json;
using MTCG;

namespace MTCG
{
    [Controller]
    public class UserController : IController
    {

        protected static IRepository<User> repo = new UserRepository();

        public UserController(IRoutingContext context) : base(context) { }

        // [Route("/users/", HTTPMethod.POST)]
        // public IResponse RegisterUser()
        // {
        //     try
        //     {
        //         var user = JsonSerializer.Deserialize<User>(context.Payload);


        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex);

        //     }
        // }


        // public IResponse AddUser()
        // {
        //     var user = JsonSerializer.Deserialize<User>(context.Payload);

        //     try
        //     {
        //         repo.Save(user);
        //         return new SuccessResponse<User>(user, "");

        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Failed to register new user. {ex.Message}");

        //         return new CustomResponse<User>(500, null, $"{ex}");
        //     }

        // }


        [Route("/users/", HTTPMethod.GET)]
        public IResponse GetAllUsers()
        {
            try
            {
                Thread.Sleep(2000);
                IEnumerable<User> users = repo.GetAll();

                return new SuccessResponse<IEnumerable<User>>(users, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get all users. {ex.Message}");

                return new ErrorResponse<User>($"Failed to get all users.");
            }

        }

        [Route("/users/{userid:int}", HTTPMethod.GET)]
        public IResponse GetUserById(int userid)
        {
            Thread.Sleep(2000);

            User? user = repo.Get(userid);

            return new SuccessResponse<User>(user, "");

        }
    }
}

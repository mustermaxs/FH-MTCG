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

        [Route("/users/", HTTPMethod.POST)]
        public IResponse AddUser()
        {
            var user = JsonSerializer.Deserialize<User>(context.Payload);

            try
            {
                repo.Save(user);
                return new SuccessResponse<User>(user, "");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                return new ErrorResponse<User>(user, $"{ex}");
            }

        }


        [Route("/users/", HTTPMethod.GET)]
        public IResponse GetAllUsers()
        {
            IEnumerable<User> users = repo.GetAll();

            return new SuccessResponse<IEnumerable<User>>(users, "");
        }

        [Route("/users/{userid:int}", HTTPMethod.GET)]
        public IResponse GetUserById(int userid)
        {

            User? user = repo.Get(userid);

            return new SuccessResponse<User>(user, "");

        }
    }
}

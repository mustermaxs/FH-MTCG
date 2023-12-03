using System.Runtime.CompilerServices;
using MTCG;

namespace MTCG
{
    [Controller]
    public class UserController : IController
    {

        protected IRepository<UserModel> repo = new UserRepo();

        public UserController(IRoutingContext context) : base(context) {}


        [Route("user/{username:alpha}/age/{age:int}", HTTPMethod.GET)]
        public string Index(string username, int age)
        {
            return $"Username: {username}\n Age: {age}";
        }

        [Route("user/{username:alpha}", HTTPMethod.GET)]
        public string GetName(string username)
        {
            return $"Username: {username}";
        }

        [Route("user/{username:alphanum}/{a:alpha}/test/{test:int}", HTTPMethod.GET)]
        public string GetName(string username, int test, string a)
        {
            return $"Username: {username} {test} {a}";
        }
    }
}

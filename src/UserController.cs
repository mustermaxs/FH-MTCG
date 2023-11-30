using MTCG;

namespace MTCG
{
    [Controller]
    public class UserController : IController
    {
        public UserController() { }

        [Route("user/{userid:int}", HTTPMethod.GET)]
        public string Index(string username)
        {
            return $"Username: {username}";
        }

        [Route("user/{username:alpha}", HTTPMethod.GET)]
        public string GetName(string username)
        {
            return $"Username: {username}";
        }
    }
}

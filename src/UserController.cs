using MTCG;

namespace MTCG
{
    [Controller]
    public class UserController
    {
        public UserController() { }

        [Route("user/{userid:int}", HTTPMethod.GET)]
        public string Index()
        {
            return "UserController INDEX";
        }

        [Route("user/{username:alpha}", HTTPMethod.GET)]
        public string GetName()
        {
            return "Maximilian Sinnl";
        }
    }
}

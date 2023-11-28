
using MTCG;

namespace MTCG
{
    [Controller]
    public class UserController
    {
        public UserController() {}
        
        [Route("user", HTTPMethod.GET)]
        public string Index()
        {
            return "UserController INDEX";
        }

    }
}
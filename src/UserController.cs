using System.Runtime.CompilerServices;
using System.Text.Json;
using MTCG;

namespace MTCG
{
    [Controller]
    public class UserController : IController
    {

        protected IRepository<UserModel> repo = new UserRepo();

        public UserController(IRoutingContext context) : base(context) { }

        [Route("users/{userid:int}", HTTPMethod.GET)]
        public IResponse GetNameById(int userid)
        {
            var mockUser1 = new UserModel("John", "Software Developer");

            // return new Response(200, JsonSerializer.Serialize<UserModel>(mockUser1), "es geht");
            return ResponseHandler.Create<UserModel>(200, mockUser1, "es geht!!!");
        }


        [Route("users/", HTTPMethod.GET)]
        public IResponse GetNames()
        {
            var mockUser1 = new UserModel("John", "Software Developer");
            var mockUser2 = new UserModel("Alice", "Data Scientist");
            var mockUser3 = new UserModel("Bob", "UX Designer");
            var mockUser4 = new UserModel();

            var list = new List<UserModel>();
            list.Add(mockUser4);
            list.Add(mockUser1);
            list.Add(mockUser3);

            // return ResponseHandler.Create<List<UserModel>>(200, list, "Es funktioniert");
            return new Response(200, JsonSerializer.Serialize<List<UserModel>>(list), "es geht");
        }
    }
}

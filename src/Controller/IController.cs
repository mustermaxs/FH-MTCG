using System;
using System.Text.Json;

namespace MTCG
{
    public abstract class IController
    {
        // TODO
        protected IRequest request;
        protected User? LoggedInUser { get; private set; }
        protected Guid UserId {get; private set;}
        protected ResponseConfig resConfig = ConfigService.Get<ResponseConfig>();

        public IController(IRequest request)
        {
            this.request = request;

            if (request.SessionId != string.Empty && request.SessionId != null)
            {
                this.LoggedInUser = SessionManager.GetUserBySessionId(request.SessionId);
                this.UserId = LoggedInUser!.ID;
            }
            else
                this.LoggedInUser = null;
        }
    }
}

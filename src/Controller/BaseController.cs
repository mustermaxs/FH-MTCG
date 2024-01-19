using System;
using System.Text.Json;

namespace MTCG
{
    public abstract class BaseController
    {
        // TODO
        protected IRequest request;
        protected User? LoggedInUser { get; private set; }
        protected ResponseTextTranslator resConfig { get; set; }
        protected Guid UserId { get; private set; }

        public BaseController(IRequest request)
        {
            this.request = request;


            if (!SessionManager.TryGetSessionById(request.SessionId, out Session? session))
                throw new Exception("Session not found");


            this.LoggedInUser = session.User;
            this.UserId = LoggedInUser!.ID;
            resConfig = ServiceProvider.Get<ResponseTextTranslator>();
            resConfig.SetLanguage(session.User.Language);

            // this.resConfig = session.responseTxt;
        }
    }
}

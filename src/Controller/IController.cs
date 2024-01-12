using System;
using System.Text.Json;

namespace MTCG
{
    public abstract class IController
    {
        // TODO
        protected IRequest request;
        protected User? LoggedInUser { get; private set; }
        protected ResponseTextTranslator resConfig { get; private set; } = Program.services.Get<ResponseTextTranslator>();
        protected Guid UserId { get; private set; }

        public IController(IRequest request)
        {
            this.request = request;


            if (!SessionManager.TryGetSessionById(request.SessionId, out Session? session))
                throw new Exception("Session not found");

            if (session.User != null)
            {
                this.LoggedInUser = session.User;
                this.UserId = LoggedInUser!.ID;
            }

            // this.resConfig = session.responseTxt;
        }
    }
}

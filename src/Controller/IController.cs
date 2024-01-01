using System;
using System.Text.Json;

namespace MTCG
{
    public abstract class IController
    {
        // TODO
        protected IRequest request;

        public IController(IRequest request)
        {
            this.request = request;
        }
    }
}

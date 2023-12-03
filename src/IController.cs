using System;
using System.Text.Json;

namespace MTCG
{
    public abstract class IController
    {
        // TODO
        protected IRoutingContext context;

        public IController(IRoutingContext context)
        {
            this.context = context;
        }
    }
}

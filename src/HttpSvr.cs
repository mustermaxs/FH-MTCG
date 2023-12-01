using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;



namespace MTCG
{
    /// <summary>This class implements a HTTP server.</summary>
    public sealed class HttpServer
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP listener.</summary>
        private TcpListener? _Listener;

        private IEndpointMapper? routeRegistry;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Occurs when a HTTP message has been received.</summary>
        public event IncomingEventHandler? Incoming;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public peoperties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets if the server is active.</summary>
        public bool Active { get; set; } = false;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the HTTP server.</summary>
        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new(IPAddress.Parse("127.0.0.1"), 12000);
            RegisterRoutes();
            _Listener.Start();

            byte[] buf = new byte[256];

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient();

                string data = string.Empty;
                while (client.GetStream().DataAvailable || (string.IsNullOrEmpty(data)))
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new HttpSvrEventArgs(client, data));
            }

            _Listener.Stop();
        }

        private void RegisterRoutes()
        {
            routeRegistry = EndpointMapper.GetInstance();
            var currentAssembly = Assembly.GetExecutingAssembly();
            IAttributeHandler attributeHandler = new AttributeHandler(currentAssembly);
            IRouteObtainer routeObtainer = new ReflectionRouteObtainer(attributeHandler);
            var routes = routeObtainer.ObtainRoutes();

            foreach (var route in routes)
            {
                (HTTPMethod method, string routeTemplate, Type controllerType, MethodInfo methodName) = route;
                routeRegistry.RegisterEndpoint(routeTemplate, method, controllerType, methodName);
            }
        }


        /// <summary>Stops the HTTP server.</summary>
        public void Stop()
        {
            Active = false;
        }
    }
}

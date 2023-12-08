using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;



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

        private IRouter router;


        public HttpServer(IRouter router)
        {
            this.router = router;

            try
            {
                router.RegisterRoutes();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Router failed to register routes.");
            }

        }



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
            _Listener.Start();
            var tasks = new List<Task>();

            byte[] buf = new byte[256];

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient();

                tasks.Add(Task.Run(() =>
                {
                    Console.WriteLine("RUN TASK");
                    string data = string.Empty;
                    while (client.GetStream().DataAvailable || (string.IsNullOrEmpty(data)))
                    {
                        int n = client.GetStream().Read(buf, 0, buf.Length);
                        data += Encoding.ASCII.GetString(buf, 0, n);
                    }

                    /// 07.12.2023 14:47
                    /// IMPROVE RoutingContext nimmt zu viele Argumente entgegen.
                    /// Setter sind ok. zusammmen mit HttpSvrEventArgs wirkts aber etwas
                    /// redundant
                    var svrEventArgs = new HttpSvrEventArgs(client, data);
                    var requestBuilder = new RequestBuilder();

                    IRequest request = requestBuilder
                    .WithHeaders(svrEventArgs.Headers)
                    .WithHttpMethod(svrEventArgs.Method)
                    .WithPayload(svrEventArgs.Payload)
                    .WithRoute(svrEventArgs.Path)
                    .Build();

                    IResponse response = router.HandleRequest(ref request);

                    svrEventArgs.Reply(response);
                }));
            }
            Task t = Task.WhenAll(tasks);

            try
            {
                t.Wait();
            }
            catch { }
            if (t.Status == TaskStatus.RanToCompletion)
            {
                _Listener.Stop();

            }
        }

        protected void SetUpRouter()
        {
            router.RegisterRoutes();
        }

        /// <summary>Stops the HTTP server.</summary>
        public void Stop()
        {
            Active = false;
        }
    }
}

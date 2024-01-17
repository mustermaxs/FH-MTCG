using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;



namespace MTCG
{
    /// <summary>This class implements a HTTP server.</summary>
    public class HttpServer
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP listener.</summary>
        private TcpListener? _Listener;


        private IRouter router;

        private ServerConfig serverConfig = ServiceProvider.Get<ServerConfig>();
        private List<Task> tasks = new List<Task>();



        public HttpServer(IRouter router)
        {
            this.router = router;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Occurs when a HTTP message has been received.</summary>
        // public event IncomingEventHandler? Incoming;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public peoperties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets if the server is active.</summary>
        public bool Active { get; set; } = false;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the HTTP server.</summary>
        public async void Run()
        {
            try
            {
                if (Active) return;

                Active = true;
                _Listener = new(IPAddress.Parse(serverConfig.SERVER_IP), serverConfig.SERVER_PORT);
                _Listener.Start();
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                CancellationToken token = cancellationToken.Token;

                while (Active)
                {
                    TcpClient client = _Listener.AcceptTcpClient();
                    tasks.Add(Task.Run(async () => {
                        var svrEventArgs = GetHttpEventArgs(client);
                        var request = CreateRequestObjAndInitSession(svrEventArgs);
                        var response = await router.HandleRequest(request);                        
                        svrEventArgs.Reply(response);
                        }, token));

                }

                await Task.WhenAll(tasks);

                if (tasks.All(t => t.Status == TaskStatus.RanToCompletion))
                    _Listener.Stop();
            }
            catch
            {
                foreach (var t in tasks)
                {
                    t.Dispose();
                }

                _Listener.Stop();
            }

        }

        protected HttpSvrEventArgs GetHttpEventArgs(TcpClient client)
        {
            string data = string.Empty;
            byte[] buf = new byte[serverConfig.BufferSize];

            while (client.GetStream().DataAvailable || (string.IsNullOrEmpty(data)))
            {
                int n = client.GetStream().Read(buf, 0, buf.Length);
                data += Encoding.ASCII.GetString(buf, 0, n);
            }

            return new HttpSvrEventArgs(client, data);
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////


        protected IRequest CreateRequestObjAndInitSession(HttpSvrEventArgs svrEventArgs)
        {
            Session session;
            var clientPort = svrEventArgs.GetHeader("Port");
            var clientIp = svrEventArgs.GetHeader("IP");
            string? authToken = svrEventArgs.GetHeader("Authorization") ?? string.Empty;
            var request = new RequestBuilder();
            string sessionId;

            request
                .WithHeaders(svrEventArgs.Headers)
                .WithHttpMethod(svrEventArgs.Method)
                .WithPayload(svrEventArgs.Payload)
                .WithRoute(svrEventArgs.Path);

            // get session if user already logged in
            if (authToken != string.Empty && SessionManager.TryGetSessionWithToken(authToken, out session))
                sessionId = session.SessionId;

            else
                sessionId = SessionManager.CreateAnonymSessionReturnId();

            request.WithSessionId(sessionId);

            return request.Build();
        }

        /// <summary>Stops the HTTP server.</summary>
        public void Stop()
        {
            Active = false;
        }
    }
}

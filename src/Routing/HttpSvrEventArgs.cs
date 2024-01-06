using System;
using System.Net;
using System.Net.Sockets;
using System.Text;



namespace MTCG
{
    /// <summary>This class provides HTTP server event arguments.</summary>
    public class HttpSvrEventArgs : EventArgs
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected members                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP client.</summary>
        protected TcpClient _Client;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="client">TCP client object.</param>
        /// <param name="plainMessage">HTTP plain message.</param>
        public HttpSvrEventArgs(TcpClient client, string plainMessage)
        {
            _Client = client;


            PlainMessage = plainMessage;
            Payload = string.Empty;

            string[] lines = plainMessage.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            bool inheaders = true;
            List<HttpHeader> headers = new();

            var ep = client.Client.RemoteEndPoint as IPEndPoint;

            if (ep != null)
            {
                var clientPort = ep.Port;
                headers.Add(new HttpHeader($"Port:{clientPort}"));
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string[] inc = lines[0].Split(' ');

                    HTTPMethod method;

                    if (Enum.TryParse(inc[0], out method))
                    {
                        Method = method;
                    }
                    else
                    {
                        throw new RouteDoesntExistException($"The HTTP method {inc[0]} was not recognized.");
                    }
                    Path = inc[1];
                }
                else if (inheaders)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else
                    {
                        headers.Add(new HttpHeader(lines[i]));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Payload)) { Payload += "\r\n"; }
                    Payload += lines[i];
                }
            }

            Headers = headers.ToArray();
        }




        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the plain HTTP message.</summary>
        public string PlainMessage
        {
            get; protected set;
        }


        /// <summary>Gets the HTTP method.</summary>
        public virtual HTTPMethod Method
        {
            get; protected set;
        }


        /// <summary>Gets the request path.</summary>
        public virtual string Path
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP headers.</summary>
        public virtual HttpHeader[] Headers
        {
            get; protected set;
        }


        /// <summary>Gets the HTTP payload.</summary>
        public virtual string Payload
        {
            get; protected set;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns a reply to the HTTP request.</summary>
        /// <param name="status">Status code.</param>
        /// <param name="payload">Payload.</param>
        public virtual void Reply(int status, string description, string? payload = null)
        {
            string protocol = "HTTP/1.1";
            string statusMsg = string.Empty;

            if (description == "" || description == null)
            {
                switch (status)
                {
                    case 200:
                        statusMsg = "OK"; break;
                    case 400:
                        statusMsg = "Bad Request"; break;
                    case 401:
                        statusMsg = "Unauthorized"; break;
                    case 404:
                        statusMsg = "Not Found"; break;
                    case 500:
                        statusMsg = "Internal Server Error"; break;
                    default:
                        statusMsg = "I'm a Teapot"; break;
                }
            }
            else
            {
                statusMsg = description;
            }


            string data = $"{protocol} {status} {statusMsg}\n";

            if (string.IsNullOrEmpty(payload))
            {
                data += "Content-Length: 0\n";
            }
            data += "Content-Type: text/plain\n\n";

            if (!string.IsNullOrEmpty(payload)) { data += payload; }

            byte[] buf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(buf, 0, buf.Length);
            _Client.Close();
            _Client.Dispose();
        }



        public virtual void Reply(IResponse response)
        {
            string protocol = "HTTP/1.1";
            var payload = response.PayloadAsJson();
            var status = response.StatusCode;
            string statusMsg =
                string.IsNullOrEmpty(response.Description) ?
                GetDefaultResponseMsgForStatus(status) :
                response.Description;

            string data = $"{protocol} {status} {statusMsg}\n";

            if (string.IsNullOrEmpty(payload))
            {
                data += "Content-Length: 0\n";
            }
            data += $"Content-Type: {response.ContentType}\n\n";

            if (!string.IsNullOrEmpty(payload)) { data += payload; }

            byte[] buf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(buf, 0, buf.Length);
            _Client.Close();
            _Client.Dispose();
        }




        /// <summary>
        /// If not status message was provided for the server response,
        /// this method is supposed to return a default response message
        /// to be sent to the client.
        /// </summary>
        /// <param name="status">
        /// HTTP status code.
        /// </param>
        protected string GetDefaultResponseMsgForStatus(int status)
        {
            switch (status)
            {
                case 200:
                    return "OK";
                case 400:
                    return "Bad Request";
                case 401:
                    return "Unauthorized";
                case 404:
                    return "Not Found";
                case 500:
                    return "Internal Server Error";
                default:
                    return "I'm a Teapot";
            }
        }
    }
}

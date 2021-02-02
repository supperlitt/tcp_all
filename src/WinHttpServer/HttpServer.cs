using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinHttpServer
{
    public class HttpServer
    {
        public event Func<string, string, string> Handler = null;

        public void Start(int port)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, 10000));
            server.Listen(1000);

            Thread t = new Thread(Execute);
            t.IsBackground = true;
            t.Start(server);
        }

        private void Execute(Object obj)
        {
            Socket server = obj as Socket;
            while (true)
            {
                Socket client = server.Accept();

                Thread t = new Thread(SingleExecute);
                t.IsBackground = true;
                t.Start(client);
            }
        }

        private void SingleExecute(object obj)
        {
            Socket client = obj as Socket;
            byte[] buffer = new byte[8192];
            client.ReceiveBufferSize = 8192;
            int count = client.Receive(buffer);
            if (count > 0)
            {
                string data = Encoding.UTF8.GetString(buffer, 0, count);
                if (data.StartsWith("GET "))
                {
                    Regex regex = new Regex(@"GET /(?<action>[^?]+)\?+(?<args>[^\s]{0,})");
                    string action = regex.Match(data).Groups["action"].Value;
                    string args = regex.Match(data).Groups["args"].Value;

                    string result = HandleRequest(action, args);
                    string body = create_body(result);

                    client.Send(Encoding.UTF8.GetBytes(body));
                }
                else if (data.StartsWith("POST "))
                {
                    Regex regex = new Regex(@"POST /(?<action>[^?]+)[\s\S]+\r\n\r\n(?<args>[^\s]{0,})");
                    string action = regex.Match(data).Groups["action"].Value;
                    string args = regex.Match(data).Groups["args"].Value;

                    string result = HandleRequest(action, args);
                    string body = create_body(result);

                    client.Send(Encoding.UTF8.GetBytes(body));
                }
            }

            client.Shutdown(SocketShutdown.Both);
        }

        private string HandleRequest(string action, string args)
        {
            if (Handler != null)
            {
                return Handler(action, args);
            }

            return string.Empty;
        }

        private string create_body(string data)
        {
            return string.Format(@"HTTP/1.1 200 OK
Content-Type: text/html
Connection: close
Content-Length: {0}

{1}", Encoding.UTF8.GetBytes(data).Length, data);
        }
    }
}

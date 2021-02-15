using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConWebSocketServer
{
    class Program
    {
        static readonly string magisk = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        static void Main(string[] args)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, 8800));
            server.Listen(1000);

            Thread t = new Thread(Execute);
            t.IsBackground = true;
            t.Start(server);

            Console.WriteLine("服务已经启动\r\n");
            Console.ReadLine();
        }

        private static void Execute(Object obj)
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

        private static void SingleExecute(object obj)
        {
            Socket client = obj as Socket;
            bool is_send = false;
            while (true)
            {
                byte[] buffer = new byte[1024];
                int count = client.Receive(buffer, SocketFlags.None);
                if (count > 0)
                {
                    if (!is_send)
                    {
                        string str = Encoding.UTF8.GetString(buffer, 0, count);
                        Regex regex = new Regex(@"Sec-WebSocket-Key: (?<key>[^\s]+)");
                        string key = regex.Match(str).Groups["key"].Value;

                        SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                        var result = sha1.ComputeHash(Encoding.UTF8.GetBytes(key + magisk));
                        string sec_WebSocket_Accept = Convert.ToBase64String(result);

                        string result_data = string.Format(@"HTTP/1.1 101 Switching Protocols
Upgrade: websocket
Connection: Upgrade
Sec-WebSocket-Accept: {0}

", sec_WebSocket_Accept);

                        client.Send(Encoding.UTF8.GetBytes(result_data), SocketFlags.None);

                        is_send = true;
                        Thread t = new Thread(RecvExecute);
                        t.IsBackground = true;
                        t.Start(client);
                    }
                }
            }
        }

        // A server must not mask any frames that it sends to the client.

        private static void RecvExecute(object obj)
        {
            Socket client = obj as Socket;
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(1000);
                byte[] send_data = crete_websocket_package(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                client.Send(send_data, SocketFlags.None);
            }
        }

        private static byte[] crete_websocket_package(string text)
        {
            byte[] text_data = Encoding.UTF8.GetBytes(text);
            List<byte> list = new List<byte>();
            byte mask = 0x0;
            list.Add(0x80 | 0x1);
            if (text_data.Length < 126)
            {
                list.Add((byte)(mask | text_data.Length));
            }
            else if (text_data.Length >= 126 && text_data.Length <= 65535)
            {
                list.Add((byte)(mask | 126));
                list.AddRange(short2bytes(text_data.Length));
            }
            else
            {
                list.Add((byte)(mask | 127));
                list.AddRange(int2bytes(text_data.Length));
            }

            if (mask == 0x80)
            {
                byte[] mask_key = new byte[] { 0x1, 0x2, 0x3, 0x4 };
                list.AddRange(mask_key);
                for (int i = 0; i < text_data.Length; i++)
                {
                    text_data[i] = (byte)(mask_key[i % 4] ^ text_data[i]);
                }
            }

            list.AddRange(text_data);

            return list.ToArray();
        }

        private static byte[] short2bytes(int len)
        {
            byte[] result = new byte[2];
            result[0] = (byte)((len >> 8) & 0xFF);
            result[1] = (byte)(len & 0xFF);

            return result;
        }

        private static byte[] int2bytes(int len)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((len >> 24) & 0xFF);
            result[1] = (byte)((len >> 16) & 0xFF);
            result[2] = (byte)((len >> 8) & 0xFF);
            result[3] = (byte)(len & 0xFF);

            return result;
        }
    }
}

/*
GET / HTTP/1.1
Host: 121.40.165.18:8800
Connection: Upgrade
Pragma: no-cache
Cache-Control: no-cache
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36
Upgrade: websocket
Origin: http://www.websocket-test.com
Sec-WebSocket-Version: 13
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Sec-WebSocket-Key: bXxK24edl3XLGpSONV4CLw==
Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits

HTTP/1.1 101 Switching Protocols
Upgrade: websocket
Connection: Upgrade
Sec-WebSocket-Accept: /7zT+ubdXbRSzpNfZDZuAW6DY1s=

.~.I<b>..............................</b> <span style='font-size:20px;'>............<a style='font-weight:900;font-size:16px;color:red;' href='http://www.blue-zero.com/sf/ah/WebSocket..................v107.zip' target='_blank'>...WebSocket..................v1.0.7...[......]</a>................................................</span>.~..<b>..............................</b> <span style='font-size:20px;'>............<a style='font-weight:900;font-size:16px;color:#009688;' href='https://www.lan-lin.com' target='_blank'>...............[......]</a> ..................................................................</span>.~..<b style='color:#ff9900;'>......(......)</b>...halo~ ..................Websocket......................................................<span style='color:red;'>............</span>............~..#....6...6....4<b>....................................</b>111111111
*/

/*
char peer0_1[] = { 0x81, 0x89, 0x23, 0x07, 0x86, 0x80, 0x12, 0x36, 0xb7, 0xb1, 0x12, 0x36, 0xb7, 0xb1, 0x12 };
char peer1_2[] = {
0x81, 0x34, 0x3c, 0x62, 0x3e, 0xe4, 0xbb, 0x8e, 
0xe6, 0x9c, 0x8d, 0xe5, 0x8a, 0xa1, 0xe7, 0xab, 
0xaf, 0xe8, 0xbf, 0x94, 0xe5, 0x9b, 0x9e, 0xe4, 
0xbd, 0xa0, 0xe5, 0x8f, 0x91, 0xe7, 0x9a, 0x84, 
0xe6, 0xb6, 0x88, 0xe6, 0x81, 0xaf, 0xef, 0xbc, 
0x9a, 0x3c, 0x2f, 0x62, 0x3e, 0x31, 0x31, 0x31, 
0x31, 0x31, 0x31, 0x31, 0x31, 0x31 };

 */
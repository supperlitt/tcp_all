using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConUDPServer
{
    class Program
    {
        private static Socket udpServer = null;

        static void Main(string[] args)
        {
            udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpServer.Bind(new IPEndPoint(IPAddress.Any, 10000));

            Thread t = new Thread(Execute);
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("服务器已经启动");
            Console.ReadLine();
        }

        private static void Execute()
        {
            byte[] buffer = new byte[1024];
            EndPoint end = new IPEndPoint(IPAddress.Any, 0);
            int count = udpServer.ReceiveFrom(buffer, ref end);
            if (count > 0)
            {
                udpServer.SendTo(Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), end);
            }
        }
    }
}

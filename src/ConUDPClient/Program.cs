using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConUDPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.103"), 10000));

            client.Send(new byte[1] { 0x01 }, SocketFlags.None);

            byte[] buffer = new byte[1024];
            int count = client.Receive(buffer);
            if (count > 0)
            {
                Console.WriteLine("收到服务器数据 " + Encoding.UTF8.GetString(buffer, 0, count));
            }

            Console.ReadLine();
        }
    }
}

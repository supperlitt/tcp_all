using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConTCPCapture
{
    class Program
    {
        private static Queue<byte[]> dataList = new Queue<byte[]>();
        private static Socket sock = null;

        static void Main(string[] args)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            sock.Bind(new IPEndPoint(IPAddress.Parse("192.168.0.103"), 0));
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            sock.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[4]);

            Thread t = new Thread(Execute);
            t.IsBackground = true;
            t.Start();

            Thread t1 = new Thread(HandleExecute);
            t1.IsBackground = true;
            t1.Start();

            Console.WriteLine("Start..");
            Console.ReadLine();
        }

        private static void Execute()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    int count = sock.Receive(buffer);
                    if (count > 0)
                    {
                        int header_len = (buffer[0] & 0xf) * 4;
                        if (buffer[9] == (int)ProtocolType.Tcp)
                        {
                            if (count > header_len)
                            {
                                byte[] tcp_data = new byte[count - header_len];
                                Array.Copy(buffer, header_len, tcp_data, 0, tcp_data.Length);

                                dataList.Enqueue(tcp_data);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private static void HandleExecute()
        {
            while (true)
            {
                try
                {
                    var tcp_data = dataList.Dequeue();
                    if (tcp_data != null)
                    {
                        int src_port = ((int)tcp_data[0] << 8) + (int)tcp_data[1];
                        int target_port = ((int)tcp_data[2] << 8) + (int)tcp_data[3];
                        if (src_port == 80 || target_port == 80)
                        {
                            int header_len = (tcp_data[12] & 0xf) * 4;
                            if (tcp_data.Length > header_len)
                            {
                                byte[] http_data = new byte[tcp_data.Length - header_len];
                                Array.Copy(tcp_data, header_len, http_data, 0, http_data.Length);

                                Console.WriteLine(Encoding.UTF8.GetString(http_data));
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConWebSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.103"), 8800));
            client.Send(Encoding.UTF8.GetBytes(string.Format(@"GET / HTTP/1.1
Host: 192.168.0.103:8800
Sec-WebSocket-Key: {0}

", Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")))));

            byte[] buffer = new byte[1024];
            int count = client.Receive(buffer, SocketFlags.None);
            if (count > 0)
            {
                Console.WriteLine("rece " + Encoding.UTF8.GetString(buffer, 0, count));
                while (true)
                {
                    buffer = new byte[1024];
                    count = client.Receive(buffer, SocketFlags.None);
                    if (count > 0)
                    {
                        // 时间？
                        int type = buffer[0] & 0xF;
                        if (type == 0x1)
                        {
                            int mask = buffer[1] & 0x80;
                            int len = buffer[1] & 0x7F;
                            int data_length = 0;

                            int index = 2;
                            if (len < 126)
                            {
                                // 这就是长度了
                                data_length = len;
                            }
                            else if (len == 126)
                            {
                                // +2
                                data_length += ((int)buffer[2] << 8);
                                data_length += ((int)buffer[3]);
                                index += 2;
                            }
                            else
                            {
                                // +4;
                                data_length += ((int)buffer[2] << 24);
                                data_length += ((int)buffer[3] << 16);
                                data_length += ((int)buffer[4] << 8);
                                data_length += ((int)buffer[5]);
                                index += 4;
                            }

                            byte[] mask_key = new byte[4];
                            if (mask == 0x80)
                            {
                                mask_key = new byte[4] { buffer[index], buffer[index + 1], buffer[index + 2], buffer[index + 3] };
                                index += 4;
                            }

                            if (count >= (data_length + index))
                            {
                                // 全部都有了
                                byte[] real_data = buffer.ToList().Skip(index).ToList().ToArray();
                                if (mask == 0x80)
                                {
                                    for (int i = 0; i < real_data.Length; i++)
                                    {
                                        real_data[i] = (byte)(mask_key[i % 4] ^ real_data[i]);
                                    }
                                }

                                Console.WriteLine("收到数据：" + Encoding.UTF8.GetString(real_data, 0, data_length));
                            }
                            else
                            {
                                byte[] data_all = new byte[data_length];
                                byte[] temp = new byte[1024];
                                count = client.Receive(temp, SocketFlags.None);

                                // TODO:接受所有数据，然后进行处理。
                                // .....
                            }
                        }
                    }
                }
            }
        }
    }
}

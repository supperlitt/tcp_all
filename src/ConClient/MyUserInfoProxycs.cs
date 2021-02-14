using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConClient
{
    public class MyUserInfoProxycs : IUserInfo
    {
        public bool login(string name, string pwd)
        {
            List<ArgTypeInfo> argList = new List<ArgTypeInfo>();
            argList.Add(new ArgTypeInfo() { arg_type = Return_Type.String, value = name });
            argList.Add(new ArgTypeInfo() { arg_type = Return_Type.String, value = pwd });
            byte[] send_data = create_send_package("IUserInfo", "login", 2, Return_Type.Bool, argList);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.103"), 10086));
            socket.Send(send_data);

            byte[] buffer = new byte[1024];
            int count = socket.Receive(buffer);
            if (count > 0)
            {
                socket.Shutdown(SocketShutdown.Both);
                Return_Type type = (Return_Type)buffer[0];
                int result = buffer[1];
                if (result == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            throw new Exception("超时");
        }

        private static byte[] create_send_package(string interface_name, string method_name, int args_length, Return_Type return_type, List<ArgTypeInfo> argList)
        {
            List<byte> list = new List<byte>();
            byte[] inter_data = Encoding.UTF8.GetBytes(interface_name);
            list.Add((byte)inter_data.Length);
            list.AddRange(inter_data);

            byte[] method_data = Encoding.UTF8.GetBytes(method_name);
            list.Add((byte)method_data.Length);
            list.AddRange(method_data);

            list.Add((byte)args_length);
            list.Add((byte)return_type);

            foreach (var item in argList)
            {
                list.Add((byte)item.arg_type);
                if (item.arg_type == Return_Type.String)
                {
                    string value = item.value as string;
                    byte[] value_data = Encoding.UTF8.GetBytes(value);

                    byte[] int_data = int2bytes(value_data.Length);
                    list.AddRange(int_data);
                    list.AddRange(value_data);
                }
                else if (item.arg_type == Return_Type.Int)
                {
                    int value = Convert.ToInt32(item.value);
                    byte[] value_data = int2bytes(value);
                    list.AddRange(value_data);
                }
                else if (item.arg_type == Return_Type.Bool)
                {
                    bool value = Convert.ToBoolean(item.value);
                    byte value_data = value ? (byte)1 : (byte)0;

                    list.Add(value_data);
                }
            }

            return list.ToArray();
        }

        private static byte[] int2bytes(int value)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((value >> (3 * 8)) & 0xFF);
            result[1] = (byte)((value >> (2 * 8)) & 0xFF);
            result[2] = (byte)((value >> (1 * 8)) & 0xFF);
            result[3] = (byte)(value & 0xFF);

            return result;
        }

        public static int bytes2int(byte[] data)
        {
            int result = 0;
            result += (int)(data[0] << (3 * 8));
            result += (int)(data[1] << (2 * 8));
            result += (int)(data[2] << (1 * 8));
            result += (int)(data[3]);

            return result;
        }
    }

    public class ArgTypeInfo
    {
        /// <summary>
        /// 0-void
        /// 1-int
        /// 2-bool
        /// 3-string
        /// </summary>
        public Return_Type arg_type { get; set; }

        public object value { get; set; }
    }

    public enum Return_Type
    {
        Void = 0,
        Int = 1,
        Bool = 2,
        String = 3
    }
}

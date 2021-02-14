using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ConServer
{
    class Program
    {
        private static Socket server = null;

        static void Main(string[] args)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, 10086));
            server.Listen(100);

            Thread t = new Thread(Execute);
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("服务器已经启动");
            Console.ReadLine();
        }

        private static void Execute()
        {
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

            byte[] buffer = new byte[1024];
            int count = client.Receive(buffer);
            if (count > 0)
            {
                MemoryStream ms = new MemoryStream(buffer, 0, count);
                BinaryReader br = new BinaryReader(ms);

                int inter_length = br.ReadByte();
                byte[] inter_data = br.ReadBytes(inter_length);

                int method_length = br.ReadByte();
                byte[] method_data = br.ReadBytes(method_length);

                int arg_length = br.ReadByte();
                Return_Type return_type = (Return_Type)br.ReadByte();

                List<object> argList = new List<object>();
                for (int i = 0; i < arg_length; i++)
                {
                    Return_Type arg_type = (Return_Type)br.ReadByte();
                    if (arg_type == Return_Type.String)
                    {
                        byte[] length_data = br.ReadBytes(4);
                        int str_length = bytes2int(length_data);
                        byte[] str_data = br.ReadBytes(str_length);
                        argList.Add(Encoding.UTF8.GetString(str_data));
                    }
                    else if (arg_type == Return_Type.Int)
                    {
                        byte[] length_data = br.ReadBytes(4);
                        int str_length = bytes2int(length_data);
                        argList.Add(str_length);
                    }
                    else if (arg_type == Return_Type.Bool)
                    {
                        byte bool_data = br.ReadByte();
                        argList.Add(bool_data == 1 ? true : false);
                    }
                }

                Type invokeType = null;
                Type[] types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    Type[] interfaces = type.GetInterfaces();
                    foreach (var face in interfaces)
                    {
                        if (face.Name == Encoding.UTF8.GetString(inter_data))
                        {
                            invokeType = type;
                            break;
                        }
                    }

                    if (invokeType != null)
                    {
                        break;
                    }
                }

                if (invokeType != null)
                {
                    MethodInfo invokeMethod = null;
                    MethodInfo[] methods = invokeType.GetMethods();
                    foreach (var m in methods)
                    {
                        if (m.Name == Encoding.UTF8.GetString(method_data))
                        {
                            invokeMethod = m;
                            break;
                        }
                    }

                    if (invokeMethod != null)
                    {
                        object thisObj = Activator.CreateInstance(invokeType);
                        object return_obj = invokeMethod.Invoke(thisObj, argList.ToArray());

                        List<byte> returnList = new List<byte>();
                        returnList.Add((byte)Return_Type.Void);
                        if (return_type == Return_Type.Int)
                        {
                            int value = Convert.ToInt32(return_obj);
                            byte[] value_data = int2bytes(value);
                            returnList.AddRange(value_data);
                        }
                        else if (return_type == Return_Type.Bool)
                        {
                            bool value = Convert.ToBoolean(return_obj);
                            returnList.Add(value ? (byte)1 : (byte)0);
                        }
                        else if (return_type == Return_Type.String)
                        {
                            string value = return_obj as string;
                            byte[] value_data = Encoding.UTF8.GetBytes(value);
                            byte[] value_length = int2bytes(value_data.Length);
                            returnList.AddRange(value_length);
                            returnList.AddRange(value_data);
                        }

                        client.Send(returnList.ToArray(), SocketFlags.None);

                        client.Shutdown(SocketShutdown.Both);
                    }
                }
            }
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
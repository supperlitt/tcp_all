using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConRPCServer
{
    public class ServiceHelpercs
    {
        public static byte[] Handle(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            int inter_len = br.ReadByte();
            string inter_name = Encoding.UTF8.GetString(br.ReadBytes(inter_len));

            int method_len = br.ReadByte();
            string method_name = Encoding.UTF8.GetString(br.ReadBytes(method_len));

            int args_length = br.ReadByte();
            int return_type = br.ReadByte();

            List<object> list = new List<object>();
            for (int i = 0; i < args_length; i++)
            {
                int arg_type = br.ReadByte();
                if (arg_type == 1)
                {
                    byte[] values = br.ReadBytes(4);
                    list.Add(bytes2int(values));
                }
                else if (arg_type == 2)
                {
                    bool value = br.ReadByte() == 1;
                    list.Add(value);
                }
                else if (arg_type == 3)
                {
                    int str_len = bytes2int(br.ReadBytes(4));
                    string str = Encoding.UTF8.GetString(br.ReadBytes(str_len));
                    list.Add(str);
                }
            }

            Type inter_type = null;
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var ts = type.GetInterfaces();
                foreach (var t in ts)
                {
                    if (t.Name == inter_name)
                    {
                        inter_type = type;
                        break;
                    }
                }
            }

            MethodInfo invokeMethod = null;
            if (inter_type != null)
            {
                var methods = inter_type.GetMethods();
                foreach (var method in methods)
                {
                    if (method.Name == method_name)
                    {
                        invokeMethod = method;
                        break;
                    }
                }
            }

            if (invokeMethod != null)
            {
                Object thisObj = Activator.CreateInstance(inter_type);
                object result = invokeMethod.Invoke(thisObj, list.ToArray());
                if (return_type == 1)
                {
                    int value = Convert.ToInt32(result);

                    return int2bytes(value);
                }
                else if (return_type == 2)
                {
                    return new byte[1] { Convert.ToBoolean(result) ? (byte)1 : (byte)0 };
                }
                else if (return_type == 2)
                {
                    List<byte> result_data = new List<byte>();
                    var str = (result == null ? "" : result.ToString());
                    var data = Encoding.UTF8.GetBytes(str);

                    result_data.AddRange(int2bytes(data.Length));
                    result_data.AddRange(data);

                    return result_data.ToArray();
                }
            }

            return new byte[1] { 0xFF };
        }

        public static byte[] int2bytes(int len)
        {
            byte[] data_len = new byte[4];
            data_len[0] = (byte)((len >> 8 * 3) & 0xFF);
            data_len[1] = (byte)((len >> 8 * 2) & 0xFF);
            data_len[2] = (byte)((len >> 8 * 1) & 0xFF);
            data_len[3] = (byte)(len & 0xFF);

            return data_len;
        }

        public static int bytes2int(byte[] buffer)
        {
            int value = 0;
            value += (int)(buffer[0] << (8 * 3));
            value += (int)(buffer[1] << (8 * 2));
            value += (int)(buffer[2] << (8 * 1));
            value += (int)(buffer[3]);

            return value;
        }
    }
}

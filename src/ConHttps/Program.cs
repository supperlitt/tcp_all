using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ConHttps
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse("118.31.180.41"), 443));

            NetworkStream ns = new NetworkStream(client, true);
            SslStream ss = new SslStream(ns, false, new RemoteCertificateValidationCallback(CallBack));
            ss.AuthenticateAsClient("www.cnblogs.com", new X509CertificateCollection(), System.Security.Authentication.SslProtocols.Tls12, false);
            ss.Write(Encoding.UTF8.GetBytes(@"GET / HTTP/1.1
Accept: text/html, application/xhtml+xml, */*
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko
Accept-Encoding: deflate
Host: www.cnblogs.com
Connection: Keep-Alive
Accept-Language: zh-CN

"));
            ss.Flush();

            byte[] buffer = new byte[8192];
            int count = ss.Read(buffer, 0, buffer.Length);
            if (count > 0)
            {
                string str = Encoding.UTF8.GetString(buffer, 0, count);
            }
        }

        static bool CallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}

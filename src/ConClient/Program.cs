using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IUserInfo proxy = new MyUserInfoProxycs();
            bool login1 = proxy.login("test", "111222");
            Console.WriteLine("login1 " + login1);
            bool login2 = proxy.login("test", "123");
            Console.WriteLine("login2 " + login2);

            Console.ReadLine();
        }
    }
}

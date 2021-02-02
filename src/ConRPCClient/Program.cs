using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ConRPCClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IMyService service = new MyServiceProxy();
            DateTime startTime = DateTime.Now;
            int result = service.add(123, 321);

            int min_seconds = (int)(DateTime.Now - startTime).TotalMilliseconds;

            Console.WriteLine(result + " 耗时 " + min_seconds);
            Console.ReadLine();
        }
    }
}

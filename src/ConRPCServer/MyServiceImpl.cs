using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConRPCServer
{
    public class MyServiceImpl : IMyService
    {
        public int add(int x, int y)
        {
            return x + y;
        }

        public bool login(string name, string pwd)
        {
            if (name == "test" && pwd == "123456")
            {
                return true;
            }

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConServer
{
    public class MyUserInfoImpl : IUserInfo
    {
        public bool login(string name, string pwd)
        {
            if (name == "test" && pwd == "123")
            {
                return true;
            }

            return false;
        }
    }
}

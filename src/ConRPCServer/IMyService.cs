﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConRPCServer
{
    public interface IMyService
    {
        int add(int x, int y);

        bool login(string name, string pwd);
    }
}

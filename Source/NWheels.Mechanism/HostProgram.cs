using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Mechanism
{
    public class HostProgram
    {
        public HostProgram(string[] args)
        {
        }

        public int Run()
        {
            return 0;
        }

        public int Run(Action code)
        {
            code();
            return 0;
        }

        public int Run<T>(Action<T> code)
        {
            return 0;
        }

        public int Run<T1, T2>(Action<T1, T2> code)
        {
            return 0;
        }

        public int Run<T1, T2, T3>(Action<T1, T2, T3> code)
        {
            return 0;
        }
    }
}

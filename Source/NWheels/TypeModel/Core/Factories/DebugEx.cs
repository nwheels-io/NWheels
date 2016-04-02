using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core.Factories
{
    public static class DebugEx
    {
        public static void WriteLine(string message, params object[] args)
        {
            Debug.WriteLine(message, args);
        }
    }
}

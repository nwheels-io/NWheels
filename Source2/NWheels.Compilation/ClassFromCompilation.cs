using NWheels.Kernel;
using System;

namespace NWheels.Compilation
{
    public class ClassFromCompilation
    {
        public string C()
        {
            var k = new ClassFromKernel();
            return k.K() + "CCC";
        }
    }
}

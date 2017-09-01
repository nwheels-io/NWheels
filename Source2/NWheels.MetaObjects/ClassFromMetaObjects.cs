using NWheels.Compilation;
using NWheels.Kernel;
using System;

namespace NWheels.MetaObjects
{
    public class ClassFromMetaObjects
    {
        public string MK()
        {
            var k = new ClassFromKernel();
            return "MMM" + k.K();
        }

        public string MC()
        {
            var c = new ClassFromCompilation();
            return "MMM" + c.C();
        }
    }
}

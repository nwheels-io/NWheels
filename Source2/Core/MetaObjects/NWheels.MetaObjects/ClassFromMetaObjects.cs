using NWheels.Compilation;
using System;

namespace NWheels.MetaObjects
{
    public class ClassFromMetaObjects
    {
        public string MK()
        {
            return "MMMKKK";
        }

        public string MC()
        {
            var c = new ClassFromCompilation();
            return "MMM" + c.C();
        }
    }
}

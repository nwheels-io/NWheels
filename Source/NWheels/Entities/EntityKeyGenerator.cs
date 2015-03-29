using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public static class EntityKeyGenerator
    {
        public class NoneAttribute : Attribute { }
        public class RandomAttribute : Attribute { }
        public class SequentialAttribute : Attribute { }
        public class SuccessiveAttribute : Attribute { }

        public class CustomAttribute : Attribute
        {
            public CustomAttribute(Type keyGeneratorType)
            {
            }
        }
    }
}

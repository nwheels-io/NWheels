using NWheels.Injection.Mechanism;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Policy
{
    public static class ComponentParameter
    {
        public static T Default<T>()
        {
            return default(T);
        }

        public static T Value<T>(T value)
        {
            return value;
        }
    }
}

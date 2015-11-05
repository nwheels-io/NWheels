using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public static class EntityImplementation
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerOnNewAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerOnValidateInvariantsAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerBeforeSaveAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerAfterSaveAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerBeforeDeleteAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerAfterDeleteAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerInsteadOfSaveAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class TriggerInsteadOfDeleteAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class DependencyPropertyAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class EntityStatePropertyAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class PropertyModifiedIndicatorAttribute : Attribute
        {
        }
    }
}

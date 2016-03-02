using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Caliburn.Validation {
    internal static class DynamicGetter {
        public static Func<object, object> From(PropertyInfo property) {
            var action = Cache.GetValueOrNull(property);
            if (action != null) return action;
            action = CompileGetter(property);
            Cache.AddOrReplace(property, action);
            return action;
        }

        private static Func<object, object> CompileGetter(PropertyInfo property) {
            var instance = Expression.Parameter(typeof (object), "instance");
            var typedInstance = Expression.Convert(instance, property.DeclaringType);
            var propertyValue = Expression.Property(typedInstance, property);
            var body = Expression.Convert(propertyValue, typeof (object));
            return Expression.Lambda<Func<object, object>>(body, instance).Compile();
        }

        #region Inner Types

        private static class Cache {
            private static readonly IDictionary<PropertyInfo, Func<object, object>> Storage =
                new Dictionary<PropertyInfo, Func<object, object>>();

            public static Func<object, object> GetValueOrNull(PropertyInfo key) {
                Func<object, object> func;
                lock (Storage) {
                    Storage.TryGetValue(key, out func);
                }
                return func;
            }

            public static void AddOrReplace(PropertyInfo key, Func<object, object> func) {
                lock (Storage) {
                    Storage[key] = func;
                }
            }
        }

        #endregion
    }
}

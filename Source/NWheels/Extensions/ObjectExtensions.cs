using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.TypeModel.Core;

namespace NWheels.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToStringOrDefault<T>(this T value, string defaultValue = null) where T : class
        {
            return (value != null ? value.ToString() : defaultValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToStringOrDefault<T>(this T? value, string defaultValue = null) where T : struct
        {
            return (value.HasValue ? value.Value.ToString() : defaultValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T AsOrNull<T>(this object obj) where T : class
        {
            var objAsT = obj as T;

            if ( objAsT != null )
            {
                return objAsT;
            }

            var container = obj as IContain<T>;

            if ( container != null )
            {
                return container.GetContainedObject();
            }

            var contained = obj as IContainedIn<T>;

            if ( contained != null )
            {
                return contained.GetContainerObject();
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T As<T>(this object obj) where T : class
        {
            if ( obj == null )
            {
                throw new ArgumentNullException("obj");
            }

            var objAsT = AsOrNull<T>(obj);

            if ( objAsT != null )
            {
                return objAsT;
            }

            throw new InvalidCastException(string.Format(
                "Specified object of type '{0}' is neither compatible with, contains, or is contained by an object of type '{1}'.", 
                obj.GetType().FullName, 
                typeof(T).FullName));
        }
    }
}

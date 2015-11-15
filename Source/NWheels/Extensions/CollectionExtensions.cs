using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddIf<T>(this ICollection<T> collection, T item) where T : class
        {
            if ( item != null )
            {
                collection.Add(item);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void AddIf<T>(this ICollection<T> collection, bool condition, T item)
        {
            if ( condition )
            {
                collection.Add(item);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void AddIf<T>(this ICollection<T> collection, bool condition, Func<T> itemFactory)
        {
            if ( condition )
            {
                collection.Add(itemFactory());
            }
        }
    }
}

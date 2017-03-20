using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Concurrency.Impl;

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IWriteOnlyCollection<T> AsWriteOnly<T>(this ICollection<T> collection)
        {
            return new WriteOnlyCollectionWrapper<T>(collection);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ContainsAny<T>(this ISet<T> set, params T[] values)
        {
            return set.Intersect(values).Any();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ContainsAll<T>(this ISet<T> set, params T[] values)
        {
            return (set.Intersect(values).Count() == values.Length);
        }
    }
}

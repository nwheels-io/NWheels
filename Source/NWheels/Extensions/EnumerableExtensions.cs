using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Utilities;

namespace NWheels.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> first, IEnumerable<T> secondOrNull) where T : class
        {
            if ( secondOrNull != null )
            {
                return first.Concat(secondOrNull);
            }
            else
            {
                return first;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.Concat((IEnumerable<T>)second);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> ConcatOne<T>(this IEnumerable<T> first, T second)
        {
            return first.Concat(new T[] { second });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> ConcatOneIf<T>(this IEnumerable<T> first, T second) where T : class
        {
            if ( second != null )
            {
                return first.Concat(new T[] { second });
            }
            else
            {
                return first;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> InjectDependenciesFrom<T>(this IEnumerable<T> source, IComponentContext components)
        {
            if ( components != null && source != null )
            {
                return new ObjectUtility.DependencyInjectingEnumerable<T>(source, components);
            }
            else
            {
                return source;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;

            foreach ( var item in source )
            {
                action(item, index);
                index++;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OfMetaType<T>(this IQueryable<T> query, ITypeMetadata metaType, string derivedTypeString = null)
        {
            if (metaType.QualifiedName.EqualsIgnoreCase(derivedTypeString))
            {
                return query;
            }

            var filterMetaType = (
                string.IsNullOrEmpty(derivedTypeString)
                ? metaType
                : metaType.DerivedTypes.FirstOrDefault(t => t.QualifiedName.EqualsIgnoreCase(derivedTypeString)));

            if (filterMetaType == null)
            {
                throw new ArgumentException(string.Format(
                    "Specified entity type '{0}' is not compatible with entity '{1}'.", 
                    derivedTypeString, metaType.QualifiedName));
            }

            return filterMetaType.MakeOfType(query);
        }
    }
}

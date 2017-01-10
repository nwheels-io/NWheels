using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Structures
{
    public interface IEqualityComparerFactory
    {
        IEqualityComparer<T> GetComparer<T>();
        Type GetComparerImplementation<T>();
        void EnsureComparersImplemented(Type[] comparedTypes);
    }
}

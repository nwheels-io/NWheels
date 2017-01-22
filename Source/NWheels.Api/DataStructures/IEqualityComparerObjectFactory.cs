using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.DataStructures
{
    public interface IEqualityComparerObjectFactory
    {
        IEqualityComparer<T> GetEqualityComparer<T>();
        Type GetEqualityComparerImplementation(Type comparedType);
    }
}

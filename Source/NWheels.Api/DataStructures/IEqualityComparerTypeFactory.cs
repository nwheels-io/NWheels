using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.DataStructures
{
    public interface IEqualityComparerTypeFactory
    {
        IEqualityComparer<T> GetEqualityComparer<T>();
        Type GetEqualityComparerImplementation(Type comparedType);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryProduct
    {
        T CreateInstance<T>(
            int constructorIndex);

        T CreateInstance<T, TArg1>(
            int constructorIndex,
            TArg1 arg1);

        T CreateInstance<T, TArg1, TArg2>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2);

        T CreateInstance<T, TArg1, TArg2, TArg3>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3);

        T CreateInstance<T, TArg1, TArg2, TArg3, TArg4>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        T CreateInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

        T CreateInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

        T CreateInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);

        T CreateInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);

        T CreateInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, 
            object[] theRest);

        ITypeKey Key { get; }
        Type RunTimeType { get; }
    }
}

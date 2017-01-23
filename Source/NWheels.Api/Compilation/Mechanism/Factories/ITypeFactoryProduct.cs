using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryProduct
    {
        TypeKey Key { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITypeFactoryProduct<TArtifact> : ITypeFactoryProduct
    {
        TArtifact Artifact { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IRuntimeTypeFactoryArtifact
    {
        T NewInstance<T>(
            int constructorIndex);

        T NewInstance<T, TArg1>(
            int constructorIndex,
            TArg1 arg1);

        T NewInstance<T, TArg1, TArg2>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2);

        T NewInstance<T, TArg1, TArg2, TArg3>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3);

        T NewInstance<T, TArg1, TArg2, TArg3, TArg4>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        T NewInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

        T NewInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

        T NewInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);

        T NewInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);

        T NewInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, 
            object[] rest);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        T GetOrCreateSingleton<T>(
            int constructorIndex);

        T GetOrCreateSingleton<T, TArg1>(
            int constructorIndex,
            TArg1 arg1);

        T GetOrCreateSingleton<T, TArg1, TArg2>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3, TArg4>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3, TArg4, TArg5>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);

        T GetOrCreateSingleton<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            int constructorIndex,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8,
            object[] rest);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type RunTimeType { get; }
        object SingletonInstance { get; }
    }
}

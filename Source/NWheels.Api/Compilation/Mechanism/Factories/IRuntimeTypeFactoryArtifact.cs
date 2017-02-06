using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface IRuntimeTypeFactoryArtifact
    {
        IRuntimeTypeFactoryArtifact<T> For<T>();
        TypeKey TypeKey { get; }
        Type RunTimeType { get; }
        object SingletonInstance { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IRuntimeTypeFactoryArtifact<T> : IRuntimeTypeFactoryArtifact
    {
        T NewInstance();

        T NewInstance<TArg1>(
            TArg1 arg1);

        T NewInstance<TArg1, TArg2>(
            TArg1 arg1, TArg2 arg2);

        T NewInstance<TArg1, TArg2, TArg3>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3);

        T NewInstance<TArg1, TArg2, TArg3, TArg4>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

        T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

        T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);

        T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);

        T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8,
            object[] rest);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        T GetOrCreateSingleton();

        T GetOrCreateSingleton<TArg1>(
            TArg1 arg1);

        T GetOrCreateSingleton<TArg1, TArg2>(
            TArg1 arg1, TArg2 arg2);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);

        T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8,
            object[] rest);
    }
}

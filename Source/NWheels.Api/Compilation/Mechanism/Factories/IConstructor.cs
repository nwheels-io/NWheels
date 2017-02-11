using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface IConstructor<T>
    {
        T NewInstance();
        T GetOrCreateSingleton();
    }
    public interface IConstructor<TArg1, T>
    {
        T NewInstance(TArg1 arg1);
        T GetOrCreateSingleton(TArg1 arg1);
    }
    public interface IConstructor<TArg1, TArg2, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, TArg4, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);
    }
    public interface IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TRest, T>
    {
        T NewInstance(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TRest rest);
        T GetOrCreateSingleton(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TRest rest);
    }
}

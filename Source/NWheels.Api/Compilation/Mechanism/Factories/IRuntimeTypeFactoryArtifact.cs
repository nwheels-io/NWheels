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
        IConstructor<T> Constructor();
        IConstructor<TArg1, T> Constructor<TArg1>();
        IConstructor<TArg1, TArg2, T> Constructor<TArg1, TArg2>();
        IConstructor<TArg1, TArg2, TArg3, T> Constructor<TArg1, TArg2, TArg3>();
        IConstructor<TArg1, TArg2, TArg3, TArg4, T> Constructor<TArg1, TArg2, TArg3, TArg4>();
        IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, T> Constructor<TArg1, TArg2, TArg3, TArg4, TArg5>();
        IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T> Constructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>();
        IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, T> Constructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>();
        IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, T> Constructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>();
        IConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TRest, T> Constructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TRest>();
    }
}

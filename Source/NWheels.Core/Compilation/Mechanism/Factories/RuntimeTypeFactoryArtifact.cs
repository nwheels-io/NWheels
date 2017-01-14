using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class RuntimeTypeFactoryArtifact : IRuntimeTypeFactoryArtifact
    {
        public Type RunTimeType => throw new NotImplementedException();

        public T GetInstance<T>(bool singleton, int constructorIndex)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1>(bool singleton, int constructorIndex, TArg1 arg1)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3, TArg4>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(bool singleton, int constructorIndex, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, object[] theRest)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class RuntimeTypeFactoryArtifact : IRuntimeTypeFactoryArtifact
    {
        private readonly object _singletonInstanceSyncRoot = new object();
        private object _singletonInstance = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected RuntimeTypeFactoryArtifact(Type runtimeType)
        {
            var typeKeyAttribute = this.GetType().GetTypeInfo().GetCustomAttribute<TypeKeyAttribute>();

            if (typeKeyAttribute == null)
            {
                throw new InvalidOperationException(
                    $"Runtime type factory artifact mush have {nameof(TypeKeyAttribute)} when using parameterless constructor: {this.GetType().FullName}");
            }

            this.TypeKey = typeKeyAttribute.ToTypeKey();
            this.RunTimeType = runtimeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected RuntimeTypeFactoryArtifact(TypeKey typeKey, Type runtimeType)
        {
            this.TypeKey = typeKey;
            this.RunTimeType = runtimeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IRuntimeTypeFactoryArtifact<T> For<T>()
        {
            return (IRuntimeTypeFactoryArtifact<T>)this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey TypeKey { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type RunTimeType { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object SingletonInstance => _singletonInstance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void CreateSingletonInstance(Func<object> factory)
        {
            lock (_singletonInstanceSyncRoot)
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = factory();
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuntimeTypeFactoryArtifact<T> : RuntimeTypeFactoryArtifact, IRuntimeTypeFactoryArtifact<T>
    {
        private readonly object _singletonInstanceSyncRoot = new object();
        private object _singletonInstance = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected RuntimeTypeFactoryArtifact(Type runtimeType)
            : base(runtimeType)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected RuntimeTypeFactoryArtifact(TypeKey typeKey, Type runtimeType)
            : base(typeKey, runtimeType)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton()
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance());
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1>(TArg1 arg1)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1>(arg1));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2>(
                    
                    arg1, arg2));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3>(
                    arg1, arg2, arg3));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3, TArg4>(
                    arg1, arg2, arg3, arg4));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5>(
                    arg1, arg2, arg3, arg4, arg5));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
                    arg1, arg2, arg3, arg4, arg5, arg6));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
                    arg1, arg2, arg3, arg4, arg5, arg6, arg7));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
                    arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetOrCreateSingleton<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, 
            object[] rest)
        {
            if (_singletonInstance == null)
            {
                CreateSingletonInstance(() => NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
                    arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, 
                    rest));
            }
            return (T)_singletonInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance()
        {
            throw new NotImplementedException(
                $"Factory method not implemented for parameterless constructor.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1>(TArg1 arg1)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor ({typeof(TArg1)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor ({typeof(TArg1)}, {typeof(TArg2)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor ({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor ({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor ({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}, {typeof(TArg5)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor " + 
                $"({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}, {typeof(TArg5)}, {typeof(TArg6)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor " +
                $"({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}, {typeof(TArg5)}, {typeof(TArg6)}, {typeof(TArg7)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor " +
                $"({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}, {typeof(TArg5)}, {typeof(TArg6)}, {typeof(TArg7)}, {typeof(TArg8)}).");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual T NewInstance<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, 
            object[] rest)
        {
            throw new NotImplementedException(
                $"Factory method not implemented for constructor " +
                $"({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}, {typeof(TArg5)}, {typeof(TArg6)}, {typeof(TArg7)}, {typeof(TArg8)}, object[]).");
        }
    }
}

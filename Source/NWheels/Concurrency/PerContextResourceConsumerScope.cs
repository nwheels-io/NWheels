using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public abstract class PerContextResourceConsumerScope<T> : IResourceConsumerScopeHandle
    {
        private readonly PerContextResourceConsumerScope<T> _outerScope;
        private readonly ContextAnchor<PerContextResourceConsumerScope<T>> _anchor;
        private readonly T _resource;
        private readonly bool _externallyOwned;
        private readonly int _nestingLevel;
        private bool _disposed = false;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PerContextResourceConsumerScope(
            ContextAnchor<PerContextResourceConsumerScope<T>> anchor,
            Func<IResourceConsumerScopeHandle, T> resourceFactory,
            bool externallyOwned)
        {
            _anchor = anchor;
            _outerScope = anchor.Current;
            anchor.Current = this;

            _externallyOwned = externallyOwned;

            if ( _outerScope != null )
            {
                _resource = _outerScope.Resource;
                _nestingLevel = _outerScope.NestingLevel + 1;
            }
            else
            {
                _resource = resourceFactory(this);
                _nestingLevel = 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDisposable

        public void Dispose()
        {
            if ( _disposed )
            {
                return;
            }

            if ( _outerScope != null )
            {
                _anchor.Current = _outerScope;
            }
            else
            {
                _anchor.Current = null;
                DisposeResource();
            }

            _disposed = true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Resource
        {
            get
            {
                return _resource;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int NestingLevel
        {
            get
            {
                return _nestingLevel;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsOutermost
        {
            get
            {
                return (_outerScope == null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsInnermost
        {
            get
            {
                return (_anchor.Current == this);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResourceConsumerScopeHandle Outermost
        {
            get
            {
                return (_outerScope != null ? _outerScope.Outermost : this);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResourceConsumerScopeHandle Innermost
        {
            get
            {
                return _anchor.Current;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected bool Disposed
        {
            get { return _disposed; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DisposeResource()
        {
            if ( !_externallyOwned )
            {
                var disposableResource = (_resource as IDisposable);

                if ( disposableResource != null )
                {
                    disposableResource.Dispose();
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class CallContextResourceConsumerScope<T> : PerContextResourceConsumerScope<T>
    {
        public CallContextResourceConsumerScope(Func<IResourceConsumerScopeHandle, T> resourceFactory, bool externallyOwned = false)
            : base(new LogicalCallContextAnchor<PerContextResourceConsumerScope<T>>(), resourceFactory, externallyOwned)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T CurrentResource
        {
            get
            {
                var currentScope = new LogicalCallContextAnchor<PerContextResourceConsumerScope<T>>().Current;

                if ( currentScope != null )
                {
                    return currentScope.Resource;
                }
                else
                {
                    return default(T);
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ThreadStaticResourceConsumerScope<T> : PerContextResourceConsumerScope<T>
    {
        public ThreadStaticResourceConsumerScope(Func<IResourceConsumerScopeHandle, T> resourceFactory, bool externallyOwned = false)
            : base(new ThreadStaticAnchor<PerContextResourceConsumerScope<T>>(), resourceFactory, externallyOwned)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T CurrentResource
        {
            get
            {
                var currentScope = new ThreadStaticAnchor<PerContextResourceConsumerScope<T>>().Current;

                if ( currentScope != null )
                {
                    return currentScope.Resource;
                }
                else
                {
                    return default(T);
                }
            }
        }
    }
}

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
        private readonly bool _forceNewResource;
        private bool _disposed = false;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PerContextResourceConsumerScope(
            ContextAnchor<PerContextResourceConsumerScope<T>> anchor,
            T resource,
            bool externallyOwned)
            : this(
                anchor,
                resourceFactory: h => resource, 
                externallyOwned: externallyOwned,
                forceNewResource: true)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PerContextResourceConsumerScope(
            ContextAnchor<PerContextResourceConsumerScope<T>> anchor,
            Func<IResourceConsumerScopeHandle, T> resourceFactory,
            bool externallyOwned,
            bool forceNewResource)
        {
            _anchor = anchor;
            _outerScope = anchor.Current;

            _externallyOwned = externallyOwned;
            _forceNewResource = forceNewResource;

            if (_outerScope != null)
            {
                if (forceNewResource)
                {
                    _resource = resourceFactory(this);
                    _outerScope.NotifyResourceActiveScopeChanged(currentScopeIsActive: false);
                    this.NotifyResourceActiveScopeChanged(currentScopeIsActive: true);
                }
                else
                {
                    _resource = _outerScope.Resource;
                }

                _nestingLevel = _outerScope.NestingLevel + 1;
            }
            else
            {
                _resource = resourceFactory(this);
                _nestingLevel = 0;
                this.NotifyResourceActiveScopeChanged(currentScopeIsActive: true);
            }

            anchor.Current = this;
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

                if ( _forceNewResource )
                {
                    this.NotifyResourceActiveScopeChanged(currentScopeIsActive: false);
                    _outerScope.NotifyResourceActiveScopeChanged(currentScopeIsActive: true);
                    DisposeResource();
                }
            }
            else
            {
                this.NotifyResourceActiveScopeChanged(currentScopeIsActive: false);
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

        public bool ForceNewResource
        {
            get { return _forceNewResource; }
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void NotifyResourceActiveScopeChanged(bool currentScopeIsActive)
        {
            var handler = (_resource as IScopedConsumptionResource);

            if ( handler != null )
            {
                handler.ActiveScopeChanged(currentScopeIsActive);
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class CallContextResourceConsumerScope<T> : PerContextResourceConsumerScope<T>
    {
        public CallContextResourceConsumerScope(T resource, bool externallyOwned = false)
            : base(new LogicalCallContextAnchor<PerContextResourceConsumerScope<T>>(), resource, externallyOwned)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CallContextResourceConsumerScope(Func<IResourceConsumerScopeHandle, T> resourceFactory, bool externallyOwned = false, bool forceNewResource = false)
            : base(new LogicalCallContextAnchor<PerContextResourceConsumerScope<T>>(), resourceFactory, externallyOwned, forceNewResource)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Clear()
        {
            new LogicalCallContextAnchor<PerContextResourceConsumerScope<T>>().Clear();
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
        public ThreadStaticResourceConsumerScope(T resource, bool externallyOwned = false)
            : base(new ThreadStaticAnchor<PerContextResourceConsumerScope<T>>(), resource, externallyOwned)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadStaticResourceConsumerScope(Func<IResourceConsumerScopeHandle, T> resourceFactory, bool externallyOwned = false, bool forceNewResource = false)
            : base(new ThreadStaticAnchor<PerContextResourceConsumerScope<T>>(), resourceFactory, externallyOwned, forceNewResource)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Clear()
        {
            new ThreadStaticAnchor<PerContextResourceConsumerScope<T>>().Clear();
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

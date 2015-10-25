using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Breeze.WebApi2;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.TypeModel.Core;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeEndpointQueryable<TContract, TDomain, TPersistable> : IQueryable<TDomain>
    {
        private readonly IQueryable<TContract> _source;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;
        private readonly Expression _expression;
        private readonly InterceptingQueryProvider _queryProvider;
        private readonly Type _persistableObjectFactoryType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeEndpointQueryable(IQueryable<TContract> source, ITypeMetadataCache metadataCache)
        {
            _source = source;
            _metadataCache = metadataCache;
            _metaType = metadataCache.GetTypeMetadata(typeof(TContract));
            _expression = Expression.Constant(this);
            _queryProvider = new InterceptingQueryProvider(this);
            _persistableObjectFactoryType = source.As<IEntityRepository>().PersistableObjectFactoryType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEnumerable

        public IEnumerator<TDomain> GetEnumerator()
        {
            var actualEnumerator = _source.GetEnumerator();

            return new DelegatingTransformingEnumerator<TContract, TDomain>(
                actualEnumerator,
                item => {
                    return (TDomain)item.As<IDomainObject>();
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IQueryable

        public Expression Expression 
        {
            get
            {
                return _expression;        
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ElementType 
        {
            get
            {
                return typeof(TDomain);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryProvider Provider
        {
            get
            {
                return _queryProvider;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadataCache MetadataCache
        {
            get { return _metadataCache; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata MetaType
        {
            get { return _metaType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TContract> Source
        {
            get { return _source; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type PersistableObjectFactoryType
        {
            get { return _persistableObjectFactoryType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQueryProvider : IQueryProvider
        {
            private readonly BreezeEndpointQueryable<TContract, TDomain, TPersistable> _ownerQueryable;
            private readonly IQueryProvider _actualQueryProvider;
            private readonly BreezeQueryExpressionSpecializer<TContract, TDomain, TPersistable> _expressionSpecializer;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQueryProvider(BreezeEndpointQueryable<TContract, TDomain, TPersistable> ownerQueryable)
            {
                _ownerQueryable = ownerQueryable;
                _actualQueryProvider = _ownerQueryable.Source.Provider;
                _expressionSpecializer = new BreezeQueryExpressionSpecializer<TContract, TDomain, TPersistable>(_ownerQueryable);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IQueryProvider Members

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery<TContract>(specializedExpression);
                return new InterceptingQuery<TElement, TContract>(query, _ownerQueryable);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable CreateQuery(Expression expression)
            {
                throw new NotSupportedException();
                //var specializedExpression = _expressionSpecializer.Specialize(expression);
                //var query = _actualQueryProvider.CreateQuery(specializedExpression);
                //return new InterceptingQuery<TImpl>(query, this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TResult Execute<TResult>(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                TResult result;

                result = _actualQueryProvider.Execute<TResult>(specializedExpression);

                if ( result == null || result.GetType().IsValueType )
                {
                    return result;
                }

                var domainObject = result.AsOrNull<IDomainObject>();

                if ( domainObject != null )
                {
                    return (TResult)domainObject;
                }
                else
                {
                    return result;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Execute(Expression expression)
            {
                var result = _actualQueryProvider.Execute(expression);
                
                if ( result == null || result.GetType().IsValueType )
                {
                    return result;
                }

                return result.AsOrNull<IDomainObject>() ?? result;
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQuery<TOuter, TInner> : IOrderedQueryable<TOuter>
        {
            private readonly IQueryable<TInner> _underlyingQuery;
            private readonly BreezeEndpointQueryable<TContract, TDomain, TPersistable> _ownerQueryable;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(
                IQueryable<TInner> underlyingQuery,
                BreezeEndpointQueryable<TContract, TDomain, TPersistable> ownerQueryable)
            {
                _underlyingQuery = underlyingQuery;
                _ownerQueryable = ownerQueryable;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<TOuter> GetEnumerator()
            {
                var actualResults = _underlyingQuery.GetEnumerator();

                return new DelegatingTransformingEnumerator<TInner, TOuter>(
                    actualResults,
                    item => {
                        return (TOuter)item.As<IDomainObject>();
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ElementType
            {
                get
                {
                    var elementType = _underlyingQuery.ElementType;
                    return elementType;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression Expression
            {
                get
                {
                    var expression = _underlyingQuery.Expression;
                    return expression;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryProvider Provider
            {
                get
                {
                    var provider = new InterceptingQueryProvider(_ownerQueryable);
                    return provider;
                }
            }
        }
    }
}

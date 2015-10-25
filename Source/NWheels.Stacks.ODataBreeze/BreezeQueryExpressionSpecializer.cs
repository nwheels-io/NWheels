using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeQueryExpressionSpecializer<TContract, TDomain, TPersistable>
    {
        private readonly BreezeEndpointQueryable<TContract, TDomain, TPersistable> _ownerQueryable;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeQueryExpressionSpecializer(BreezeEndpointQueryable<TContract, TDomain, TPersistable> ownerQueryable)
        {
            _ownerQueryable = ownerQueryable;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Expression Specialize(Expression general)
        {
            if ( general == null )
            {
                return null;
            }

            var visitor = new SpecializingVisitor(this);
            var specialized = visitor.Visit(general);
            return specialized;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeEndpointQueryable<TContract, TDomain, TPersistable> OwnerQueryable
        {
            get { return _ownerQueryable; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SpecializingVisitor : ExpressionVisitor
        {
            private readonly BreezeQueryExpressionSpecializer<TContract, TDomain, TPersistable> _ownerSpecializer;
            private readonly ITypeMetadata _thisTypeMetadata;
            private readonly ITypeMetadataCache _metadataCache;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SpecializingVisitor(BreezeQueryExpressionSpecializer<TContract, TDomain, TPersistable> ownerSpecializer)
            {
                _ownerSpecializer = ownerSpecializer;
                _thisTypeMetadata = ownerSpecializer.OwnerQueryable.MetaType;
                _metadataCache = ownerSpecializer.OwnerQueryable.MetadataCache;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if ( node.Method.IsGenericMethod && node.Method.GetGenericArguments().Any(ShouldReplaceType) )
                {
                    var replacedTypeArguments = node.Method.GetGenericArguments().Select(t => t.Replace(ShouldReplaceType, GetReplacingType)).ToArray();
                    
                    var specialized = Expression.Call(
                        _ownerSpecializer.Specialize(node.Object),
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(replacedTypeArguments),
                        node.Arguments.Select(arg => _ownerSpecializer.Specialize(arg)));

                    return specialized;
                }
                else if ( 
                    node.Method.DeclaringType != null && 
                    node.Method.DeclaringType.IsGenericType &&
                    node.Method.DeclaringType.GetGenericArguments().Any(ShouldReplaceType) )
                {
                    var replacedTypeArguments = node.Method.DeclaringType.GetGenericArguments().Select(t => t.Replace(ShouldReplaceType, GetReplacingType)).ToArray();
                    var replacedDeclaringType = node.Method.DeclaringType.GetGenericTypeDefinition().MakeGenericType(replacedTypeArguments);
                    var replacedParameterTypes = node.Method.GetParameters().Select(p => p.ParameterType.Replace(ShouldReplaceType, GetReplacingType)).ToArray();
                    var replacedMethod = replacedDeclaringType.GetMethod(node.Method.Name, replacedParameterTypes);
                    var specialized = Expression.Call(
                        _ownerSpecializer.Specialize(node.Object),
                        replacedMethod,
                        node.Arguments.Select(arg => _ownerSpecializer.Specialize(arg)));

                    return specialized;
                }
                else //if ( node.Arguments.Any(arg => ShouldReplaceType(arg.Type)) )
                {
                    var specialized = Expression.Call(
                        _ownerSpecializer.Specialize(node.Object),
                        node.Method,
                        node.Arguments.Select(arg => _ownerSpecializer.Specialize(arg)));

                    return specialized;
                }

                //return node;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                var replacedDelegateType = typeof(T).Replace(findWhat: ShouldReplaceType, replaceWith: GetReplacingType);

                var specialized = Expression.Lambda(
                    replacedDelegateType,
                    _ownerSpecializer.Specialize(node.Body),
                    node.Parameters.Select(p => _ownerSpecializer.Specialize(p)).Cast<ParameterExpression>());

                return specialized;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if ( ShouldReplaceType(node.Type) )
                {
                    var replacingType = GetReplacingType(node.Type);
                    var replaced = Expression.Parameter(replacingType, node.Name);
                    return replaced;
                }
                else
                {
                    return base.VisitParameter(node);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if ( node.Type == this._ownerSpecializer.OwnerQueryable.GetType() )
                {
                    return _ownerSpecializer.OwnerQueryable.Source.Expression;
                }

                return base.VisitConstant(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitMember(MemberExpression node)
            {
                if ( node.Member is PropertyInfo && node.Member.DeclaringType != null && ShouldReplaceType(node.Member.DeclaringType) )
                {
                    var originalPropertyInfo = (PropertyInfo)node.Member;
                    var contractType = GetContractType(node.Member.DeclaringType);

                    if ( contractType != null )
                    {
                        var replacingPropertyInfo = _metadataCache
                            .GetTypeMetadata(contractType)
                            .GetPropertyByName(originalPropertyInfo.Name)
                            .GetImplementationBy(_ownerSpecializer.OwnerQueryable.PersistableObjectFactoryType);

                        var replaced = Expression.MakeMemberAccess(_ownerSpecializer.Specialize(node.Expression), replacingPropertyInfo);
                        return replaced;

                    }
                }

                return base.VisitMember(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitUnary(UnaryExpression node)
            {
                return base.VisitUnary(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplaceType(Type type)
            {
                if ( type == typeof(TDomain) || typeof(IDomainObject).IsAssignableFrom(type) || (type.IsInterface && _metadataCache.ContainsTypeMetadata(type)) )
                {
                    return true;
                }

                if ( type.IsGenericType )
                {
                    return type.GetGenericArguments().Any(ShouldReplaceType);
                }

                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type GetReplacingType(Type type)
            {
                var persistableFactoryType = _ownerSpecializer.OwnerQueryable.PersistableObjectFactoryType;

                if ( type == typeof(TDomain) )
                {
                    return _thisTypeMetadata.GetImplementationBy(persistableFactoryType);
                }

                if ( type.IsInterface && _metadataCache.ContainsTypeMetadata(type) )
                {
                    return _metadataCache.GetTypeMetadata(type).GetImplementationBy(persistableFactoryType);
                }

                if ( type.IsClass && typeof(IDomainObject).IsAssignableFrom(type) )
                {
                    var contractType = GetDomainObjectContractType(type);

                    if ( contractType != null )
                    {
                        return _metadataCache.GetTypeMetadata(contractType).GetImplementationBy(persistableFactoryType);
                    }
                }

                if ( type.IsGenericType )
                {
                    var openType = type.GetGenericTypeDefinition();
                    var replacingTypeArguments = type.GetGenericArguments().Select(t => ShouldReplaceType(t) ? GetReplacingType(t) : t).ToArray();
                    return openType.MakeGenericType(replacingTypeArguments);
                }

                return type;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type GetContractType(Type type)
            {
                if ( type.IsInterface && _metadataCache.ContainsTypeMetadata(type) )
                {
                    return type;
                }

                if ( typeof(IDomainObject).IsAssignableFrom(type) )
                {
                    return GetDomainObjectContractType(type);
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type GetDomainObjectContractType(Type domainObjectImplementationType)
            {
                return domainObjectImplementationType.GetInterfaces().FirstOrDefault(intf => 
                    _metadataCache.ContainsTypeMetadata(intf) && 
                    _metadataCache.GetTypeMetadata(intf).GetImplementationBy<DomainObjectFactory>() == domainObjectImplementationType);
            }
        }
    }
}

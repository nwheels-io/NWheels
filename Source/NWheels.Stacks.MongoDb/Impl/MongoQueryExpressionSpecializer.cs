using System;
using System.Linq;
using System.Linq.Expressions;
using NWheels.DataObjects;
using System.Reflection;
using NWheels.Extensions;

namespace NWheels.Stacks.MongoDb.Impl
{
    public class MongoQueryExpressionSpecializer
    {
        private readonly ITypeMetadata _thisTypeMetadata;
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoQueryExpressionSpecializer(ITypeMetadata thisTypeMetadata, ITypeMetadataCache metadataCache)
        {
            _thisTypeMetadata = thisTypeMetadata;
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Expression Specialize(Expression general)
        {
            var visitor = new SpecializingVisitor(this, _thisTypeMetadata, _metadataCache);
            return visitor.Visit(general);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SpecializingVisitor : ExpressionVisitor
        {
            private readonly MongoQueryExpressionSpecializer _ownerSpecializer;
            private readonly ITypeMetadata _thisTypeMetadata;
            private readonly ITypeMetadataCache _metadataCache;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SpecializingVisitor(MongoQueryExpressionSpecializer ownerSpecializer, ITypeMetadata thisTypeMetadata, ITypeMetadataCache metadataCache)
            {
                _ownerSpecializer = ownerSpecializer;
                _thisTypeMetadata = thisTypeMetadata;
                _metadataCache = metadataCache;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if ( node.Method.IsGenericMethod && node.Method.GetGenericArguments().Any(ShouldReplaceType) )
                {
                    var replacedTypeArguments = node.Method.GetGenericArguments().Select(t => t.Replace(ShouldReplaceType, GetReplacingType)).ToArray();
                    
                    return Expression.Call(
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(replacedTypeArguments),
                        node.Arguments.Select(arg => _ownerSpecializer.Specialize(arg)));
                }

                return node;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                var replacedDelegateType = typeof(T).Replace(findWhat: ShouldReplaceType, replaceWith: GetReplacingType);

                return Expression.Lambda(
                    replacedDelegateType,
                    _ownerSpecializer.Specialize(node.Body),
                    node.Parameters.Select(p => _ownerSpecializer.Specialize(p)).Cast<ParameterExpression>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitParameter(ParameterExpression node)
            {
                ITypeMetadata typeMetadata;

                if ( node.Type == _thisTypeMetadata.ContractType )
                {
                    var replaced = Expression.Parameter(_thisTypeMetadata.GetImplementationBy<MongoEntityObjectFactory>(), node.Name);
                    return replaced;
                }
                else if ( _metadataCache.TryGetTypeMetadata(node.Type, out typeMetadata) )
                {
                    var replaced = Expression.Parameter(typeMetadata.GetImplementationBy<MongoEntityObjectFactory>(), node.Name);
                    return replaced;
                }
                else
                {
                    return base.VisitParameter(node);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitMember(MemberExpression node)
            {
                var declaringType = node.Member.DeclaringType;

                if ( declaringType != null && declaringType.IsInterface && node.Member is PropertyInfo )
                {
                    ITypeMetadata typeMetadata;

                    if ( declaringType == _thisTypeMetadata.ContractType )
                    {
                        typeMetadata = _thisTypeMetadata;
                    }
                    else
                    {
                        _metadataCache.TryGetTypeMetadata(declaringType, out typeMetadata);
                    }

                    if ( typeMetadata != null )
                    {
                        var implementationPropertyInfo = typeMetadata.GetPropertyByDeclaration((PropertyInfo)node.Member).GetImplementationBy<MongoEntityObjectFactory>();
                        var replaced = Expression.MakeMemberAccess(_ownerSpecializer.Specialize(node.Expression), implementationPropertyInfo);
                        return replaced;
                    }
                }

                return base.VisitMember(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplaceType(Type type)
            {
                return (type == _thisTypeMetadata.ContractType || type.IsInterface && _metadataCache.ContainsTypeMetadata(type));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type GetReplacingType(Type type)
            {
                return _metadataCache.GetTypeMetadata(type).GetImplementationBy<MongoEntityObjectFactory>();
            }
        }
    }
}

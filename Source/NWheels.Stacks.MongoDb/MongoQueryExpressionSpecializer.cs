using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb
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
            if ( general == null )
            {
                return null;
            }

            var visitor = new SpecializingVisitor(this, _thisTypeMetadata, _metadataCache);
            var specialized = visitor.Visit(general);
            return specialized;
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
                    var replacingArguments = node.Arguments.Select(arg => _ownerSpecializer.Specialize(arg)).ToArray();

                    if ( node.Method.DeclaringType == typeof(EntityId) && replacingArguments.Length == 1 )
                    {
                        return replacingArguments[0];
                    }

                    var specialized = Expression.Call(
                        _ownerSpecializer.Specialize(node.Object),
                        node.Method,
                        replacingArguments);

                    return specialized;
                }

                return node;
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
                    ITypeMetadata metaType;

                    if ( declaringType == _thisTypeMetadata.ContractType )//|| (declaringType.IsEntityPartContract() && declaringType.IsAssignableFrom(_thisTypeMetadata.ContractType)) )
                    {
                        metaType = _thisTypeMetadata;
                    }
                    else
                    {
                        _metadataCache.TryGetTypeMetadata(declaringType, out metaType);
                    }

                    if ( metaType != null )
                    {
                        var contractPropertyInfo = (PropertyInfo)node.Member;
                        PropertyInfo implementationPropertyInfo;

                        if ( !TryGetImplementationPropertyInfo(metaType, contractPropertyInfo, out implementationPropertyInfo) )
                        {
                            if ( !TryGetImplementationPropertyInfo(_thisTypeMetadata, contractPropertyInfo, out implementationPropertyInfo) )
                            {
                                throw new Exception(string.Format(
                                    "Could not find implementation for property: {0}.{1}",
                                    contractPropertyInfo.DeclaringType.Name,
                                    contractPropertyInfo.Name));
                            }
                        }

                        var replacedTarget = _ownerSpecializer.Specialize(node.Expression);
                        var replaced = Expression.MakeMemberAccess(replacedTarget, implementationPropertyInfo);
                        return replaced;
                    }
                }

                return base.VisitMember(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitUnary(UnaryExpression node)
            {
                var replacingOperand = base.Visit(node.Operand);

                if ( replacingOperand == node.Operand )
                {
                    return node;
                }

                return Expression.MakeUnary(node.NodeType, replacingOperand, replacingOperand.Type, null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return base.VisitConstant(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override Expression VisitBinary(BinaryExpression node)
            {
                var left = base.Visit(node.Left);
                var right = base.Visit(node.Right);

                if ( left.Type != right.Type )
                {
                    if ( left.Type != node.Left.Type )
                    {
                        var leftMember = node.Left as MemberExpression;
                        if ( leftMember != null )
                        {
                            var leftProperty = leftMember.Member as PropertyInfo;

                            if ( leftProperty != null )
                            {
                                ITypeMetadata metaType;
                                
                                if ( _metadataCache.TryGetTypeMetadata(leftProperty.DeclaringType, out metaType) )
                                {
                                    var metaProperty = metaType.GetPropertyByDeclaration(leftProperty);
                                    
                                    if ( metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.StorageType != null )
                                    {
                                        var rightValue = ExpressionUtility.Evaluate(right);
                                        var convertedRightValue = metaProperty.RelationalMapping.StorageType.ContractToStorage(metaProperty, rightValue);
                                        right = Expression.Constant(convertedRightValue);
                                    
                                        var replaced = Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, method: null, conversion: null);
                                        return replaced;
                                    }
                                }
                            }
                        }
                    }
                }

                //var replaced = Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, method: null, conversion: null); 
                ////var replaced = node.Update(left, node.Conversion, right);
                //return replaced;

                return base.VisitBinary(node);
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool TryGetImplementationPropertyInfo(
                ITypeMetadata metaType,
                PropertyInfo contractPropertyInfo,
                out PropertyInfo implementationPropertyInfo)
            {
                IPropertyMetadata metaProperty;

                if ( metaType.TryGetPropertyByDeclaration(contractPropertyInfo, out metaProperty) )
                {
                    return metaProperty.TryGetImplementationBy<MongoEntityObjectFactory>(out implementationPropertyInfo);
                }
                else
                {
                    implementationPropertyInfo = null;
                    return false;
                }
            }
        }
    }
}

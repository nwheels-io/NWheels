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
                    var replacedArguments = node.Arguments.Select(arg => _ownerSpecializer.Specialize(arg)).ToArray();

                    if ( IsQueryableOrderByOrGroupByMethod(node.Method) )
                    {
                        replacedTypeArguments[1] = replacedArguments[replacedArguments.Length - 1].Type.GetGenericArguments()[0].GetGenericArguments()[1];
                    }

                    var specialized = Expression.Call(
                        _ownerSpecializer.Specialize(node.Object),
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(replacedTypeArguments),
                        replacedArguments);

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
                var specializedBody = _ownerSpecializer.Specialize(node.Body);
                var specializedParameters = node.Parameters.Select(p => _ownerSpecializer.Specialize(p)).Cast<ParameterExpression>();

                Type replacedDelegateType;

                if ( node.Body.Type == specializedBody.Type )
                {
                    replacedDelegateType = typeof(T).Replace(findWhat: ShouldReplaceType, replaceWith: GetReplacingType);
                }
                else
                {
                    var oldNewTypePairs = new[] { node.Body.Type, specializedBody.Type };
                    replacedDelegateType = typeof(T).Replace(
                        findWhat: t => ShouldReplaceType(t, oldNewTypePairs),
                        replaceWith: t => GetReplacingType(t, oldNewTypePairs));
                }

                var specialized = Expression.Lambda(
                    replacedDelegateType,
                    specializedBody,
                    specializedParameters);

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

                        if ( !implementationPropertyInfo.DeclaringType.IsAssignableFrom(replacedTarget.Type) )
                        {
                            replacedTarget = Expression.Convert(replacedTarget, implementationPropertyInfo.DeclaringType);
                        }

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

            private bool IsQueryableOrderByOrGroupByMethod(MethodInfo method)
            {
                return (
                    method.DeclaringType == typeof(Queryable) &&
                    (method.Name == "OrderBy" ||
                    method.Name == "OrderByDescending" ||
                    method.Name == "ThenBy" ||
                    method.Name == "ThenByDescending" ||
                    method.Name == "GroupBy"));
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

            private bool ShouldReplaceType(Type type, params Type[] oldNewTypePairs)
            {
                if ( type == _thisTypeMetadata.ContractType || (type.IsInterface && _metadataCache.ContainsTypeMetadata(type)) )
                {
                    return true;
                }

                var index = Array.IndexOf(oldNewTypePairs, type);
                return (index >= 0 && index < oldNewTypePairs.Length - 1);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type GetReplacingType(Type type, params Type[] oldNewTypePairs)
            {
                ITypeMetadata metaType;

                if ( _metadataCache.TryGetTypeMetadata(type, out metaType) )
                {
                    return metaType.GetImplementationBy<MongoEntityObjectFactory>();
                }
                else
                {
                    var index = Array.IndexOf(oldNewTypePairs, type);
                    
                    if ( index >= 0 && index < oldNewTypePairs.Length - 1 )
                    {
                        return oldNewTypePairs[index + 1];
                    }
                }

                return type;
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

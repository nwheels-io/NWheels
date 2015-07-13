using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.Endpoints;
using NWheels.Exceptions;
using NWheels.Extensions;
using System.Web.Http;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.AspNet
{
    public class WebApiControllerFactory : ConventionObjectFactory
    {
        public WebApiControllerFactory(DynamicModule module)
            : base(module)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type CreateControllerType(Type handlerType)
        {
            var typeKey = new TypeKey(primaryInterface: handlerType);
            return base.GetOrBuildType(typeKey).DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new ControllerConvention(context.TypeKey.PrimaryInterface)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ControllerConvention : ImplementationConvention
        {
            private readonly Type _handlerClassType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ControllerConvention(Type handlerClassType)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _handlerClassType = handlerClassType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(ApiController);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var handlerOperationMethods = TypeMemberCache.Of(_handlerClassType).SelectAllMethods(IsHttpOperationMethod).ToArray();

                using ( TT.CreateScope<TT.TService>(_handlerClassType) )
                {
                    var handlerField = writer.Field<TT.TService>("_handler");

                    writer.Constructor<TT.TService>((cw, handler) => {
                        cw.Base();
                        handlerField.Assign(handler);
                    });

                    foreach ( var handlerMethod in handlerOperationMethods )
                    {
                        var operationAttribute = handlerMethod.GetCustomAttribute<HttpOperationAttribute>();

                        writer.NewVirtualFunction<HttpResponseMessage>(handlerMethod.Name).Implement(
                            m => Attributes
                                .Set<RouteAttribute>(values => values.Arg(operationAttribute.Route))
                                .Set<HttpGetAttribute>()
                                .Set<HttpPostAttribute>(),
                            w => {
                                w.Return(handlerField.Func<HttpRequestMessage, HttpResponseMessage>(
                                    handlerMethod,
                                    w.This<ApiController>().Prop(x => x.Request)));
                            });
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsHttpOperationMethod(MethodInfo method)
            {
                if ( !method.IsPublic || !method.HasAttribute<HttpOperationAttribute>() )
                {
                    return false;
                }

                var parameters = method.GetParameters();

                if ( method.ReturnType != typeof(HttpResponseMessage) || parameters.Length != 1 || parameters[0].ParameterType != typeof(HttpRequestMessage) )
                {
                    throw new ContractConventionException(
                        typeof(ControllerConvention), 
                        _handlerClassType, method, 
                        "HTTP operations must be public methods of signature 'HttpResponseMessage Operation(HttpRequestMessage)'.");
                }

                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------


        }
    }
}

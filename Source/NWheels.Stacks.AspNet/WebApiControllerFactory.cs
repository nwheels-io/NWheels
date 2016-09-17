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
using NWheels.Hosting.Factories;
using NWheels.Logging;
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

                writer.Attribute<SecurityCheck.AllowAnonymousAttribute>();

                using ( TT.CreateScope<TT.TService>(_handlerClassType) )
                {
                    var handlerField = writer.Field<TT.TService>("_handler");
                    var loggerField = writer.Field<IWebApplicationLogger>("_logger");

                    writer.Constructor<TT.TService, ComponentAspectFactory, IWebApplicationLogger>((cw, handler, aspectFactory, logger) => {
                        cw.Base();
                        handlerField.Assign(aspectFactory.Func<Type, object>(x => x.CreateInheritor, cw.Const(_handlerClassType)).CastTo<TT.TService>());
                        loggerField.Assign(logger);
                    });

                    foreach ( var handlerMethod in handlerOperationMethods )
                    {
                        var handlerMethodCopy = handlerMethod;
                        var operationAttribute = handlerMethod.GetCustomAttribute<HttpOperationAttribute>();

                        writer.NewVirtualFunction<HttpResponseMessage>(handlerMethod.Name).Implement(
                            m => Attributes
                                .Set<RouteAttribute>(values => values.Arg(operationAttribute.Route))
                                .Set<HttpGetAttribute>()
                                .Set<HttpPostAttribute>(),
                            w => {
                                //var activityLocal = w.Local<ILogActivity>(initialValue: w.Const<ILogActivity>(null));
                                //activityLocal.Assign(loggerField.Func<string, string, ILogActivity>(
                                //    x => x.ApiControllerAction,
                                //    w.Const(_handlerClassType.Name + "." + handlerMethodCopy.Name),
                                //    w.Const(operationAttribute.Route)
                                //));
                                var responseLocal = w.Local<HttpResponseMessage>();
                                //w.Using(activityLocal).Do(() => {
                                //    w.Try(() => {
                                //        responseLocal.Assign(handlerField.Func<HttpRequestMessage, HttpResponseMessage>(
                                //            handlerMethod,
                                //            w.This<ApiController>().Prop(x => x.Request)));
                                //    }).Catch<Exception>(e => {
                                //        activityLocal.Void(x => x.Fail, e);
                                //        w.Throw();
                                //    });
                                //});
                                responseLocal.Assign(handlerField.Func<HttpRequestMessage, HttpResponseMessage>(
                                    handlerMethod,
                                    w.This<ApiController>().Prop(x => x.Request)));
                                w.Return(responseLocal);
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

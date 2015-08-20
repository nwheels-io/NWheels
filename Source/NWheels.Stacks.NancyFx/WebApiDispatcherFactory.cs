using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using Nancy;
using Nancy.ModelBinding;
using NWheels.Conventions.Core;
using NWheels.UI.ServerSide;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.NancyFx
{
    public class WebApiDispatcherFactory : ConventionObjectFactory
    {
        public WebApiDispatcherFactory(DynamicModule module, ViewModelObjectFactory objectFactory)
            : base(module, new WebApiDispatcherConvention(objectFactory))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApiDispatcherBase CreateDispatcher(Type apiContractType)
        {
            return (WebApiDispatcherBase)base.CreateInstanceOf(apiContractType).UsingDefaultConstructor();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WebApiDispatcherConvention : ImplementationConvention
        {
            private readonly ViewModelObjectFactory _objectFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WebApiDispatcherConvention(ViewModelObjectFactory objectFactory)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _objectFactory = objectFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(WebApiDispatcherBase);
                
                var apiContract = context.TypeKey.PrimaryInterface;

                foreach ( var operationMethod in TypeMemberCache.Of(apiContract).ImplementableMethods )
                {
                    var requestContractType = operationMethod.GetParameters()[0].ParameterType;
                    _objectFactory.GetOrBuildEntityImplementation(requestContractType);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var apiContract = writer.OwnerClass.Key.PrimaryInterface;

                writer
                    .DefaultConstructor()
                    .ImplementBase<WebApiDispatcherBase>()
                    .Method(x => x.RegisterOperations).Implement(w => {
                        foreach ( var operationMethod in TypeMemberCache.Of(apiContract).ImplementableMethods )
                        {
                            WriteOperationRegistration(operationMethod, apiContract, w);
                        }
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteOperationRegistration(MethodInfo operationMethod, Type apiContract, VoidMethodWriter w)
            {
                var requestContractType = operationMethod.GetParameters()[0].ParameterType;
                var requestImplType = _objectFactory.GetOrBuildEntityImplementation(requestContractType);

                using ( TT.CreateScope<TT.TRequest, TT.TRequestImpl, TT.TReply>(
                    requestContractType, requestImplType, operationMethod.ReturnType) )
                {
                    var callback = w.Local<Func<INancyModule, object, object>>();

                    callback.Assign(w.Delegate<INancyModule, object, object>((ww, module, service) => {
                        var request = ww.Local<TT.TRequestImpl>();
                        var reply = ww.Local<TT.TReply>();

                        request.Assign(Static.Func(ModuleExtensions.Bind<TT.TRequestImpl>, 
                            module, 
                            Static.Prop(() => WebApiDispatcherBase.ModelBindingConfig)));

                        using ( TT.CreateScope<TT.TService>(apiContract) )
                        {
                            reply.Assign(service.CastTo<TT.TService>().Func<TT.TRequest, TT.TReply>(operationMethod, request));
                        }

                        ww.Return(reply);
                    }));

                    w.This<WebApiDispatcherBase>().Void(x => x.RegisterOperation, w.Const(operationMethod.Name), callback);
                }
            }
        }
    }
}

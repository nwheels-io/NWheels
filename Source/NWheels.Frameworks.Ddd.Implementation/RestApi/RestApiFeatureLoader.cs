using NWheels.Compilation;
using NWheels.Injection;
using NWheels.Microservices;
using NWheels.Platform.Rest;
using System;
using System.Linq;
using System.Reflection;

namespace NWheels.Frameworks.Ddd.RestApi
{
    [FeatureLoader(Name = "RestApi")]
    public class RestApiFeatureLoader : FeatureLoaderBase
    {
        private Type[] _allTxTypes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
        {
            containerBuilder.ContributeTypeFactory<TxResourceHandlerTypeFactory, ITxResourceHandlerObjectFactory>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CompileComponents(IInternalComponentContainer input)
        {
            CompileTxHandlers(input);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeCompiledComponents(IInternalComponentContainer input, IComponentContainerBuilder output)
        {
            ContributeCompiledTxHandlers(input, output);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompileTxHandlers(IInternalComponentContainer input)
        {
            _allTxTypes = input.GetAllServices(typeof(object))
                .Where(t => t.GetTypeInfo().IsDefined(typeof(TransactionScriptComponentAttribute)))
                .ToArray();

            var handlerTypeFactory = input.Resolve<TxResourceHandlerTypeFactory>();

            foreach (var txType in _allTxTypes)
            {
                handlerTypeFactory.EnsureResourceHandlersCompiled(txType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ContributeCompiledTxHandlers(IInternalComponentContainer input, IComponentContainerBuilder output)
        {
            var handlerObjectFactory = input.Resolve<ITxResourceHandlerObjectFactory>();

            foreach (var txType in _allTxTypes)
            {
                var handlerList = handlerObjectFactory.CreateResourceHandlerList(txType);

                foreach (var handlerType in handlerList.GetHandlerTypes())
                {
                    output.Register<IRestResourceHandler>(handlerType);
                }
            }
        }
    }
}

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

        public override void CompileComponents(IComponentContainer input)
        {
            CompileTxHandlers(input);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeCompiledComponents(IComponentContainer input, IComponentContainerBuilder output)
        {
            ContributeCompiledTxHandlers(input, output);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompileTxHandlers(IComponentContainer input)
        {
            _allTxTypes = input.GetAllServiceTypes(typeof(object))
                .Where(t => t.GetTypeInfo().IsDefined(typeof(TransactionScriptComponentAttribute)))
                .ToArray();

            var handlerTypeFactory = input.Resolve<TxResourceHandlerTypeFactory>();

            foreach (var txType in _allTxTypes)
            {
                handlerTypeFactory.EnsureResourceHandlersCompiled(txType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ContributeCompiledTxHandlers(IComponentContainer input, IComponentContainerBuilder output)
        {
            var handlerObjectFactory = input.Resolve<ITxResourceHandlerObjectFactory>();

            foreach (var txType in _allTxTypes)
            {
                var handlerList = handlerObjectFactory.CreateResourceHandlerList(txType);

                foreach (var handlerType in handlerList.GetHandlerTypes())
                {
                    output.RegisterComponentType(handlerType).ForService<IRestResourceHandler>();
                }
            }
        }
    }
}

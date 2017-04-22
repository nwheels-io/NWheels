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

        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents.ContributeTypeFactory<TxResourceHandlerTypeFactory, ITxResourceHandlerObjectFactory>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CompileComponents(IComponentContainer existingComponents)
        {
            CompileTxHandlers(existingComponents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            ContributeCompiledTxHandlers(existingComponents, newComponents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CompileTxHandlers(IComponentContainer existingComponents)
        {
            _allTxTypes = existingComponents.GetAllServiceTypes(typeof(object))
                .Where(t => t.GetTypeInfo().IsDefined(typeof(TransactionScriptComponentAttribute)))
                .ToArray();

            var handlerTypeFactory = existingComponents.Resolve<TxResourceHandlerTypeFactory>();

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

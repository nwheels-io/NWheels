using NWheels.Compilation;
using NWheels.Injection;
using NWheels.Microservices;
using NWheels.Platform.Rest;
using System.Linq;
using System.Reflection;

namespace NWheels.Frameworks.Ddd.RestApi
{
    [FeatureLoader(Name = "RestApi")]
    public class RestApiFeatureLoader : FeatureLoaderBase
    {
        public override void RegisterComponents(IComponentContainerBuilder containerBuilder)
        {
            containerBuilder.ContributeTypeFactory<TxResourceHandlerTypeFactory, ITxResourceHandlerObjectFactory>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CompileComponents(IInternalComponentContainer input, IComponentContainerBuilder output)
        {
            CompileTxHandlers(input, output);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CompileTxHandlers(IInternalComponentContainer input, IComponentContainerBuilder output)
        {
            var allTxTypes = input.GetAllServices(typeof(object))
                .Where(t => t.GetTypeInfo().IsDefined(typeof(TransactionScriptComponentAttribute)))
                .ToArray();

            var handlerTypeFactory = input.Resolve<TxResourceHandlerTypeFactory>();

            foreach (var txType in allTxTypes)
            {
                handlerTypeFactory.EnsureResourceHandlersCompiled(txType);
            }

            var handlerObjectFactory = input.Resolve<ITxResourceHandlerObjectFactory>();

            foreach (var txType in allTxTypes)
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

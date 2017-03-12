using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    public interface IInternalComponentContainer : IComponentContainer
    {
        /// <summary>
        /// Returns types of all regitered services that are compatible with specified base type.
        /// </summary>
        /// <param name="baseType">
        /// A base type that filters list of registered services. To get all services, pass typeof(object) here.
        /// </param>
        IEnumerable<Type> GetAllServices(Type baseType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void Merge(IInternalComponentContainerBuilder containerBuilder);
    }
}

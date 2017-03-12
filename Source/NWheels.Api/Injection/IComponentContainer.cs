using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    //TODO: naming - better use TService than TInterface, 
    //      because components can sometimes register under their concrete types or abstract classes, 
    //      not just interfaces

    public interface IComponentContainer : IDisposable
    {
        TInterface Resolve<TInterface>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<TInterface> ResolveAll<TInterface>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns types of all regitered services that are compatible with specified base type.
        /// </summary>
        /// <param name="baseType">
        /// A base type that filters list of registered services. To get all services, pass typeof(object) here.
        /// </param>
        IEnumerable<Type> GetAllServices(Type baseType);
    }
}

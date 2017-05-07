using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    public interface IComponentContainer : IDisposable
    {
        bool TryResolve<TService>(out TService instance);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService Resolve<TService>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveNamed<TService>(string name);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<TService> ResolveAll<TService>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Returns types of all regitered services that are compatible with specified base type.
        /// </summary>
        /// <param name="baseType">
        /// A base type that filters list of registered services. To get all services, pass typeof(object) here.
        /// </param>
        IEnumerable<Type> GetAllServiceTypes(Type baseType);
    }
}

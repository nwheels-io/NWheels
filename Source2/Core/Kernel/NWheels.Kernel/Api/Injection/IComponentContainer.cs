using System;
using System.Collections.Generic;

namespace NWheels.Kernel.Api.Injection
{
    public interface IComponentContainer : IDisposable
    {
        bool TryResolve<TService>(out TService instance);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService Resolve<TService>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveWithArguments<TService, TArg1>(TArg1 arg1);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveWithArguments<TService, TArg1, TArg2>(TArg1 arg1, TArg2 arg2);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveWithArguments<TService>(params object[] arguments);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveNamed<TService>(string name);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveNamedWithArguments<TService, TArg1>(string name, TArg1 arg1);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveNamedWithArguments<TService, TArg1, TArg2>(string name, TArg1 arg1, TArg2 arg2);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TService ResolveNamedWithArguments<TService>(string name, params object[] arguments);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<TService> ResolveAll<TService>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool TryResolve(Type serviceType, out object instance);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object Resolve(Type serviceType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object ResolveWithArguments(Type serviceType, params object[] arguments);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object ResolveNamed(Type serviceType, string name);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object ResolveNamedWithArguments(Type serviceType, string name, params object[] arguments);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<object> ResolveAll(Type serviceType);

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

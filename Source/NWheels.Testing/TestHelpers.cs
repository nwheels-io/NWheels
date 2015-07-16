using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Testing
{
    public static class TestHelpers
    {
        public static Autofac.IContainer BuildComponentContainer<TService1>(params object[] instances)
        {
            return BuildComponentContainer(new[] { typeof(TService1) }, instances);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static Autofac.IContainer BuildComponentContainer<TService1, TService2>(params object[] instances)
        {
            return BuildComponentContainer(new[] { typeof(TService1), typeof(TService2) }, instances);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Autofac.IContainer BuildComponentContainer<TService1, TService2, TService3>(params object[] instances)
        {
            return BuildComponentContainer(new[] { typeof(TService1), typeof(TService2), typeof(TService3) }, instances);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Autofac.IContainer BuildComponentContainer(Type[] serviceTypes, object[] serviceInstances)
        {
            var builder = new ContainerBuilder();

            for ( int i = 0 ; i < serviceTypes.Length ; i++ )
            {
                if ( serviceInstances != null && i < serviceInstances.Length )
                {
                    builder.RegisterInstance(serviceInstances[i]).As(serviceTypes[i]);
                }
                else
                {
                    builder.RegisterType(serviceTypes[i]);
                }
            }

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UpdateComponentContainer<TService1>(IComponentContext container, params object[] instances)
        {
            UpdateComponentContainer(container, new[] { typeof(TService1) }, instances);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UpdateComponentContainer<TService1, TService2>(IComponentContext container, params object[] instances)
        {
            UpdateComponentContainer(container, new[] { typeof(TService1), typeof(TService2) }, instances);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UpdateComponentContainer<TService1, TService2, TService3>(IComponentContext container, params object[] instances)
        {
            UpdateComponentContainer(container, new[] { typeof(TService1), typeof(TService2), typeof(TService3) }, instances);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UpdateComponentContainer(IComponentContext container, params Type[] serviceTypes)
        {
            UpdateComponentContainer(container, serviceTypes, serviceInstances: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UpdateComponentContainer(IComponentContext container, Type[] serviceTypes, object[] serviceInstances)
        {
            var builder = BuildContainerBuilder(serviceTypes, serviceInstances);
            builder.Update(container.ComponentRegistry);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ContainerBuilder BuildContainerBuilder(Type[] serviceTypes, object[] serviceInstances)
        {
            var builder = new ContainerBuilder();

            for ( int i = 0 ; i < serviceTypes.Length ; i++ )
            {
                if ( serviceInstances != null && i < serviceInstances.Length )
                {
                    builder.RegisterInstance(serviceInstances[i]).As(serviceTypes[i]);
                }
                else
                {
                    builder.RegisterType(serviceTypes[i]);
                }
            }
            
            return builder;
        }
    }
}

using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Injection
{
    public abstract class AbstractUIAppInjectorPort : InjectorPort
    {
        protected AbstractUIAppInjectorPort(IComponentContainerBuilder containerBuilder, Type appComponentType) 
            : base(containerBuilder)
        {
            this.AppComponentType = appComponentType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AppComponentType { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string ApplicationName
        {
            get
            {
                return AppComponentType.Name;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WebAppInjectorPort : AbstractUIAppInjectorPort
    {
        public WebAppInjectorPort(IComponentContainerBuilder containerBuilder, Type appComponentType, string uriPathBase)
            : base(containerBuilder, appComponentType)
        {
            this.UriPathBase = uriPathBase;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPathBase { get; }
    }
}

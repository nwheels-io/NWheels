using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Breeze.ContextProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NWheels.Stacks.ODataBreeze
{
    public class EndpointBreezeConfig : BreezeConfig
    {
        #region Overrides of BreezeConfig

        protected override JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var settings = base.CreateJsonSerializerSettings();
            
            settings.Converters.Add(new DomainObjectJsonConverter());
            settings.ContractResolver = new DomainObjectContractResolver(Components.Resolve<IBreezeEndpointLogger>());
            
            return settings;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static IComponentContext Components { get; set; }
    }
}

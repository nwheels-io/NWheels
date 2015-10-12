using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breeze.ContextProvider;
using Newtonsoft.Json;

namespace NWheels.Stacks.ODataBreeze
{
    public class EndpointBreezeConfig : BreezeConfig
    {
        #region Overrides of BreezeConfig

        protected override JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var settings = base.CreateJsonSerializerSettings();
            settings.Converters.Add(new DomainObjectJsonConverter());
            return settings;
        }

        #endregion
    }
}

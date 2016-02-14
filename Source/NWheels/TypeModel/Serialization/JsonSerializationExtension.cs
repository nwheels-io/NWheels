#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NWheels.UI.Core;

namespace NWheels.TypeModel.Serialization
{
    public class JsonSerializationExtension : IJsonSerializationExtension
    {
        #region Implementation of IJsonSerializationExtension

        public void ApplyTo(JsonSerializerSettings settings)
        {
            throw new NotImplementedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class IntervalContractResolver : DefaultContractResolver
        {
            #region Overrides of DefaultContractResolver

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return new[] {
                    new JsonProperty() {
                        PropertyName = 
                    },
                    new JsonProperty() {
                        
                    } 
                };
            }

            #endregion
        }
    }
}

#endif
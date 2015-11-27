using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NWheels.UI.Core
{
    public interface IJsonSerializationExtension
    {
        void ApplyTo(JsonSerializerSettings settings);
    }
}

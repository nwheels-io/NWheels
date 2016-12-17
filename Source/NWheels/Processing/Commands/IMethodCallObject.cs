using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NWheels.Serialization;

namespace NWheels.Processing.Commands
{
    public interface IMethodCallObject
    {
        //bool CheckAuthorization(out bool authenticationRequired);
        void ExecuteOn(object target);
        object GetParameterValue(int index);
        object GetParameterValue(string name);
        void SetParameterValue(int index, object value);
        void SetParameterValue(string name, object value);
        MethodInfo MethodInfo { get; }
        IMethodCallSerializerObject Serializer { get; }
        object Result { get; }
        long CorrelationId { get; set; }
        [JsonExtensionData]
        Dictionary<string, JToken> ExtensionData { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMethodCallSerializerObject
    {
        void SerializeInput(CompactSerializationContext context);
        void SerializeOutput(CompactSerializationContext context);
        void DeserializeInput(CompactDeserializationContext context);
        void DeserializeOutput(CompactDeserializationContext context);
    }
}

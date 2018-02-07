using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Runtime.Execution;
using NWheels.Samples.HelloWorld.HelloService;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HelloTxHelloMethodInvocation : IInvocation
    {
        public InputMessage Input;
        public OutputMessage Output;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public async Task Invoke(object target)
        {
            var typedTarget = (Program.HelloTx)target;
            Output.ReturnValue = await typedTarget.Hello(Input.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        Task IInvocation.Promise => throw new NotImplementedException();

        object IInvocation.Result => Output.ReturnValue;

        Type IInvocation.TargetType => typeof(Program.HelloTx);

        MethodInfo IInvocation.TargetMethod => throw new NotImplementedException();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct InputMessage
        {
            public string Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public struct OutputMessage
        {
            public string ReturnValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class InputMessageSerializer
        {
            private static readonly Dictionary<string, MessagePropertyPopulator<JsonTextReader, InputMessage>> _s_propertyPopulatorByName =
                new Dictionary<string, MessagePropertyPopulator<JsonTextReader, InputMessage>> {
                    ["name"] = new MessagePropertyPopulator<JsonTextReader, InputMessage>((JsonTextReader json, ref InputMessage target) => {
                        target.Name = json.ReadAsString();
                        return true;
                    })
                };

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static bool DeserializeFromJson(JsonTextReader json, ref InputMessage target)
            {
                if (json.TokenType != JsonToken.StartObject)
                {
                    return false;
                }

                while (json.Read() && json.TokenType != JsonToken.EndObject)
                {
                    if (json.TokenType != JsonToken.PropertyName)
                    {
                        return false;
                    }
                    if (!_s_propertyPopulatorByName.TryGetValue(
                        (string)json.Value,
                        out MessagePropertyPopulator<JsonTextReader, InputMessage> deserializeProperty))
                    {
                        return false;
                    }
                    if (!deserializeProperty(json, ref target))
                    {
                        return false;
                    }
                }

                return true;
            }

            //TODO: all encodings are contributed by registered implementations of IMessageEncoderTypeFactory:
            //TODO: public static bool DeserializeToXml(XmlReader xml, ref OutputMessage target)
            //TODO: public static bool DeserializeToBson(BsonReader bson, ref OutputMessage target)
            //TODO: public static bool DeserializeToRawBinary(RawBinaryReader binary, ref OutputMessage target)
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class OutputMessageSerializer
        {
            public static void SerializeToJson(JsonTextWriter json, ref OutputMessage target)
            {
                json.WriteStartObject();
                json.WritePropertyName("result");
                json.WriteValue(target.ReturnValue);
                json.WriteEndObject();
            }

            //TODO: all encodings are contributed by registered implementations of IMessageEncoderTypeFactory:
            //TODO: public static void SerializeToXml(XmlWriter xml, ref OutputMessage target)
            //TODO: public static void SerializeToBson(BsonWriter bson, ref OutputMessage target)
            //TODO: public static void SerializeToRawBinary(RawBinaryWriter binary, ref OutputMessage target)
        }
    }
}
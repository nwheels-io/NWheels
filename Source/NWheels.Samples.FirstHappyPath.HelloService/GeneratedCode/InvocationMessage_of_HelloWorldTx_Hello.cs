using NWheels.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    public class InvocationMessage_of_HelloWorldTx_Hello : TaskCompletionSource<object>, IInvocationMessage
    {
        private InputMessage _input;
        private OutputMessage _output;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InvocationMessage_of_HelloWorldTx_Hello()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ref InputMessage Input
        {
            get
            {
                return ref _input;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ref OutputMessage Output
        {
            get
            {
                var awaitResult = base.Task.Result;
                return ref _output;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Task IInvocationMessage.CompletionFuture => base.Task;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IInvocationMessage.TargetType => typeof(HelloWorldTx);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        MethodInfo IInvocationMessage.TargetMethod => throw new NotImplementedException("Requires netstandard2.0"); //Module.ResolveMethod

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Exception IInvocationMessage.Exception => base.Task.Exception;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        async Task IInvocationMessage.Invoke(object target)
        {
            try
            {
                string returnValue = await ((HelloWorldTx)target).Hello(_input.Name);

                _output = new OutputMessage {
                    ReturnValue = returnValue
                };

                base.SetResult(null);
            }
            catch (Exception e)
            {
                base.SetException(e);
            }
        }

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

        public delegate bool MessagePropertyPopulator<TInput, TTarget>(TInput input, ref TTarget target);

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

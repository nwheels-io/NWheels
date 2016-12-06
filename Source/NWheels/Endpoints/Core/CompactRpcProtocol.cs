using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Serialization;

namespace NWheels.Endpoints.Core
{
    public static class CompactRpcProtocol
    {
        public static void WriteCall(IMethodCallObject call, CompactSerializationContext context)
        {
            var memberKey = context.Dictionary.LookupMemberKeyOrThrow(call.MethodInfo);
            context.Output.Write7BitInt(memberKey);
            context.Serializer.WriteObjectContents(call, context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMethodCallObject ReadCall(IMethodCallObjectFactory callFactory, CompactDeserializationContext context)
        {
            var memberKey = context.Input.Read7BitInt();
            var method = (MethodInfo)context.Dictionary.LookupMemberOrThrow(memberKey);
            
            var call = callFactory.NewMessageCallObject(method);
            context.Serializer.PopulateObject(context, call);

            return call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EndpointSerializer : IEndpointSerializer<IMethodCallObject, byte[]>
        {
            private readonly IComponentContext _components;
            private readonly IMethodCallObjectFactory _callFactory;
            private readonly CompactSerializer _serializer;
            private readonly StaticCompactSerializerDictionary _dictionary;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected EndpointSerializer(
                IComponentContext components, 
                IMethodCallObjectFactory callFactory, 
                CompactSerializer serializer, 
                params Type[] apiContracts)
            {
                _components = components;
                _callFactory = callFactory;
                _serializer = serializer;
                _dictionary = new StaticCompactSerializerDictionary();

                foreach (var contract in apiContracts)
                {
                    _dictionary.RegisterApiContract(contract);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEndpointSerializer<IMethodCallObject,byte[]>

            public byte[] Serialize(IMethodCallObject deserialized)
            {
                using (var buffer = new MemoryStream())
                {
                    using (var writer = new CompactBinaryWriter(buffer))
                    {
                        var context = new CompactSerializationContext(_serializer, _dictionary, writer);
                        WriteCall(deserialized, context);
                        writer.Flush();

                        return buffer.ToArray();
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IMethodCallObject Deserialize(byte[] serialized)
            {
                using (var buffer = new MemoryStream(serialized))
                {
                    using (var reader = new CompactBinaryReader(buffer))
                    {
                        var context = new CompactDeserializationContext(_serializer, _dictionary, reader, _components);
                        var call = ReadCall(_callFactory, context);

                        return call;
                    }
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EndpointSerializer<TApi> : EndpointSerializer
            where TApi : class
        {
            public EndpointSerializer(
                IComponentContext components, 
                IMethodCallObjectFactory callFactory, 
                CompactSerializer serializer)
                : base(components, callFactory, serializer, typeof(TApi))
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EndpointSerializer<TServerApi, TClientApi> : EndpointSerializer
            where TServerApi : class
            where TClientApi : class
        {
            public EndpointSerializer(
                IComponentContext components,
                IMethodCallObjectFactory callFactory,
                CompactSerializer serializer)
                : base(components, callFactory, serializer, typeof(TServerApi), typeof(TClientApi))
            {
            }
        }
    }
}

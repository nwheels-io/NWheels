using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Concurrency;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Serialization;

namespace NWheels.Endpoints.Core
{
    public static class CompactRpcProtocol
    {
        public const long NoCorrelationId = -1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string FaultCodeInternalError = "InternalError";
        public static readonly string FaultCodeRemoteParty = "RemotePartyFault";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WriteCall(IMethodCallObject call, CompactSerializationContext context)
        {
            var memberKey = context.Dictionary.LookupMemberKeyOrThrow(call.MethodInfo);

            context.Output.Write((byte)RpcMessageType.Call);
            context.Output.Write7BitInt(memberKey);
            context.Output.Write(call.CorrelationId);
            call.Serializer.SerializeInput(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WriteReturn(IMethodCallObject call, CompactSerializationContext context)
        {
            var memberKey = context.Dictionary.LookupMemberKeyOrThrow(call.MethodInfo);

            context.Output.Write((byte)RpcMessageType.Return);
            context.Output.Write7BitInt(memberKey);
            context.Output.Write(call.CorrelationId);
            
            if (call.CorrelationId != CompactRpcProtocol.NoCorrelationId)
            {
                call.Serializer.SerializeOutput(context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WriteFault(IMethodCallObject call, CompactSerializationContext context, string faultCode)
        {
            var memberKey = context.Dictionary.LookupMemberKeyOrThrow(call.MethodInfo);

            context.Output.Write((byte)RpcMessageType.Fault);
            context.Output.Write7BitInt(memberKey);
            context.Output.Write(call.CorrelationId);
            context.Output.WriteStringOrNull(faultCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMethodCallObject ReadCall(IMethodCallObjectFactory callFactory, CompactDeserializationContext context)
        {
            IMethodCallObject call;
            object returnValue;
            string faultCode;
            long correlationId;

            var messageType = ReadRpcMessage(
                callFactory, 
                context,
                (id, ctx) => {
                    throw new NotSupportedException();
                },
                out call, 
                out returnValue, 
                out faultCode, 
                out correlationId);

            if (messageType != RpcMessageType.Call)
            {
                throw new ProtocolViolationException("Expected message of type Call, but got: " + messageType);
            }

            return call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static RpcMessageType ReadRpcMessage(
            IMethodCallObjectFactory callFactory, 
            CompactDeserializationContext context,
            HandleReturnMessageCallback returnMessageHandler,
            out IMethodCallObject call,
            out object returnValue,
            out string returnFaultCode,
            out long correlationId)
        {
            call = null;
            returnValue = null;
            returnFaultCode = null;

            var messageType = (RpcMessageType)context.Input.ReadByte();
            var memberKey = context.Input.Read7BitInt();
            correlationId = context.Input.ReadInt64();
            
            var method = (MethodInfo)context.Dictionary.LookupMemberOrThrow(memberKey);

            switch (messageType)
            {
                case RpcMessageType.Call:
                    call = callFactory.NewMessageCallObject(method);
                    call.Serializer.DeserializeInput(context);
                    break;
                case RpcMessageType.Return:
                    call = returnMessageHandler(correlationId, context);
                    break;
                case RpcMessageType.Fault:
                    returnFaultCode = context.Input.ReadStringOrNull();
                    break;
                default:
                    throw new ProtocolViolationException("Unexpected message type code: " + messageType);
            }

            return messageType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum RpcMessageType
        {
            Call = 0,
            Return = 1,
            Fault = 2
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate IMethodCallObject HandleReturnMessageCallback(long correlationId, CompactDeserializationContext deserializationContext);

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

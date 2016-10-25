using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
    }
}

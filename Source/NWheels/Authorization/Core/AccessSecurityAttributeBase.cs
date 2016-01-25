using System;
using System.Security.Claims;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;

namespace NWheels.Authorization.Core
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class AccessSecurityAttributeBase : Attribute
    {
        public abstract void ValidateAccessOrThrow(ClaimsPrincipal principal);
        public abstract void WriteSecurityCheck(MethodWriterBase writer, IOperand<ClaimsPrincipal> principal, StaticStringsDecorator staticStrings);
    }
}
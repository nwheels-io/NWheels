using System;
using NWheels.Api.Exceptions;

namespace NWheels.Api.Logging
{
    public static class LogAs
    {

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method)]
        public class ErrorAttribute : LogMessageAttribute
        {
            public ErrorAttribute(FaultParty faultParty) 
                : base(LogLevel.Error, faultParty)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ErrorAttribute(FaultParty faultParty, object errorCode) 
                : base(LogLevel.Error, faultParty)
            {
                ErrorCode = errorCode;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object ErrorCode { get; set;}
        }
    } 
}
using NWheels.Api.Exceptions;

namespace NWheels.Api.Logging
{
    public abstract class LogMessageAttribute : System.Attribute
    {
        protected LogMessageAttribute(LogLevel logLevel, FaultParty faultParty)
        {
            this.LogLevel = logLevel;
            this.FaultParty = faultParty;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevel LogLevel { get; protected set; }
        public FaultParty FaultParty { get; protected set; }
    }
}
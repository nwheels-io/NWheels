using System;

namespace NWheels.Api.Exceptions
{
    public abstract class KnownErrorException : Exception
    {
        protected KnownErrorException(FaultParty faultParty, Type codeType, string codeString = null, string subCodeString = null)
            : base()
        {
            this.TypeString = codeType.Name;
            this.CodeString = codeString; 
            this.SubCodeString = subCodeString;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FaultParty FaultParty { get; private set; } 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string TypeString { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CodeString { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SubCodeString { get; private set; }
    }
}
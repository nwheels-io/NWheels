using System;
using NWheels.Api.Exceptions;

namespace NWheels.Api.Ddd.Exceptions
{
    public abstract class DomainErrorException : KnownErrorException
    {
        public DomainErrorException(FaultParty faultParty, Type codeType, string codeString, string subCodeString = null) 
            : base(faultParty, codeType, codeString, subCodeString)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainErrorException<TCode> : DomainErrorException
    {
        public DomainErrorException(FaultParty faultParty, TCode codeType) 
            : base(faultParty, typeof(TCode), codeType.ToString())
        {
            this.CodeType = codeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TCode CodeType { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainErrorException<TCode, TSubCode> : DomainErrorException<TCode>
    {
        public DomainErrorException(FaultParty faultParty, TCode code, TSubCode subCode) 
            : base(faultParty, code)
        {
            this.SubCode = subCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TSubCode SubCode { get; private set; }
    }
}

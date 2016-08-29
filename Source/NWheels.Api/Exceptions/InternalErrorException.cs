using System;

namespace NWheels.Api.Exceptions
{
    public class InternalErrorException<TCode> : KnownErrorException
    {
        public InternalErrorException(TCode code) 
            : base(FaultParty.Server, typeof(TCode), code.ToString())
        {
            this.Code = code;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TCode Code { get; private set;}
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class InternalErrorException<TCode, TSubCode> : InternalErrorException<TCode>
    {
        public InternalErrorException(TCode code, TSubCode subCode) 
            : base(code)
        {
            this.SubCode = subCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TSubCode SubCode { get; private set;}
    }
}

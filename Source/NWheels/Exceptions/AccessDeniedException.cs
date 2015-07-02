using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    public class AccessDeniedException : SecurityException
    {
        public AccessDeniedException()
            : base("Access denied")
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AccessDeniedException(Claim failedClaim) : this()
        {
            this.FailedClaim = failedClaim;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AccessDeniedException(object failedClaimValue) : this()
        {
            this.FailedClaimValue = failedClaimValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Claim FailedClaim { get; private set; }
        public object FailedClaimValue { get; private set; }
    }
}

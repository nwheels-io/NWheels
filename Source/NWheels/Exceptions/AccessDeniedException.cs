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

        public AccessDeniedException(string failedClaimType, string failedClaimValue) 
            : base(string.Format("Access denied. User has no [{0}:{1}]", failedClaimType, failedClaimValue))
        {
            this.FailedClaimType = failedClaimType;
            this.FailedClaimValue = failedClaimValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FailedClaimType { get; private set; }
        public string FailedClaimValue { get; private set; }
    }
}

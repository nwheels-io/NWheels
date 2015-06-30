using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Authorization
{
    public class DataPermissionClaimSet : ClaimSet
    {
        #region Overrides of ClaimSet

        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerator<Claim> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Claim this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ClaimSet Issuer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}

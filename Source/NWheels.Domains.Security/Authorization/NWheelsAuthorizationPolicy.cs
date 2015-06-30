using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Security.Principal;

namespace NWheels.Domains.Security.Authorization
{
    public class NWheelsAuthorizationPolicy : IAuthorizationPolicy
    {
        private NWheelsUserIdentity _identity;
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NWheelsAuthorizationPolicy(NWheelsUserIdentity identity)
        {
            _identity = identity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            //evaluationContext.AddClaimSet(this, new WindowsClaimSet(_identity));

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public System.IdentityModel.Claims.ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Id
        {
            get { return this.GetType().Name; }
        }
    }
}

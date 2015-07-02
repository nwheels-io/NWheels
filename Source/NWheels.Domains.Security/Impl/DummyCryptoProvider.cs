using System.Security;
using System.Text;
using NWheels.Domains.Security.Core;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Impl
{
    public class DummyCryptoProvider : ICryptoProvider
    {
        public byte[] CalculateHash(SecureString text)
        {
            return Encoding.ASCII.GetBytes(SecureStringUtility.SecureToClear(text));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool MatchHash(byte[] hash, SecureString text)
        {
            return (Encoding.ASCII.GetString(hash) == SecureStringUtility.SecureToClear(text));
        }
    }
}

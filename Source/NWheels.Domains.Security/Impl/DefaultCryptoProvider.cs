using System.Security;
using System.Security.Cryptography;
using System.Text;
using NWheels.Domains.Security.Core;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Impl
{
    public class DefaultCryptoProvider : ICryptoProvider
    {
        public byte[] CalculateHash(SecureString text, string salt)
        {
            SHA256Managed sha256 = new SHA256Managed();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salt + text.SecureToClear()));
            return hash;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool MatchHash(byte[] hash, SecureString text, string salt)
        {
            SHA256Managed sha256 = new SHA256Managed();
            var expectedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salt + text.SecureToClear()));

            if ( hash.Length != expectedHash.Length )
            {
                return false;
            }

            for ( int i = 0 ; i < hash.Length ; i++ )
            {
                if ( hash[i] != expectedHash[i] )
                {
                    return false;
                }
            }

            return true;
        }
    }
}

using System.Security;

namespace NWheels.Domains.Security.Core
{
    public interface ICryptoProvider
    {
        byte[] CalculateHash(SecureString text);
        bool MatchHash(byte[] hash, SecureString text);
    }
}

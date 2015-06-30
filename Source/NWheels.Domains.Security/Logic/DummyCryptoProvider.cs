using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Logic
{
    public class DummyCryptoProvider : ICryptoProvider
    {
        public byte[] CalculateHash(string clearText)
        {
            return Encoding.ASCII.GetBytes(clearText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool MatchHash(byte[] hash, string clearText)
        {
            return (Encoding.ASCII.GetString(hash) == clearText);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security.Logic
{
    public interface ICryptoProvider
    {
        byte[] CalculateHash(string clearText);
        bool MatchHash(byte[] hash, string clearText);
    }
}

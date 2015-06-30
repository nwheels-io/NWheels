using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security
{
    public static class PasswordEntityExtensions
    {
        public static bool IsExpired(this IPasswordEntity password, DateTime utcNow)
        {
            if ( !password.ExpiresAtUtc.HasValue )
            {
                return false;
            }

            return (utcNow > password.ExpiresAtUtc.Value);
        }
    }
}

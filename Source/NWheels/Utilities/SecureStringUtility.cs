using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Utilities
{
    public static class SecureStringUtility
    {
        public static SecureString ClearToSecure(this string clearText)
        {
            if ( clearText == null )
            {
                throw new ArgumentNullException("clearText");
            }

            unsafe
            {
                fixed ( char* passwordChars = clearText )
                {
                    var secureText = new SecureString(passwordChars, clearText.Length);
                    secureText.MakeReadOnly();
                    return secureText;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string SecureToClear(this SecureString securePassword)
        {
            if ( securePassword == null )
            {
                throw new ArgumentNullException("securePassword");
            }

            IntPtr unmanagedString = IntPtr.Zero;

            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}

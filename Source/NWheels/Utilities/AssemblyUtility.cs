using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace NWheels.Utilities
{
    public static class AssemblyUtility
    {
        public static void Redirect(string shortName, Version targetVersion, string publicKeyToken)
        {
            ResolveEventHandler handler = null;
            
            handler = (sender, args) => {
                var requestedAssembly = new AssemblyName(args.Name);
                
                if ( requestedAssembly.Name != shortName )
                {
                    return null;
                }

                requestedAssembly.Version = targetVersion;
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + publicKeyToken).GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;
                return Assembly.Load(requestedAssembly);
            };

            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }
    }
}

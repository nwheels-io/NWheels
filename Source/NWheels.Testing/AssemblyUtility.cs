using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Testing
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

                Debug.WriteLine(
                    "AssemblyUtility::Redirect >> redirecting assembly load of '{0}', loaded by '{1}', use version {2}",
                    args.Name,
                    (args.RequestingAssembly == null ? "(unknown)" : args.RequestingAssembly.FullName),
                    targetVersion);

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Utilities
{
    public static class PathUtility
    {
        private static readonly string _s_binFolderPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static PathUtility()
        {
            if ( !string.IsNullOrEmpty(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath) )
            {
                _s_binFolderPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            }
            else if ( !string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory) )
            {
                _s_binFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                _s_binFolderPath = Path.GetDirectoryName(typeof(PathUtility).Assembly.Location);
            }
        }
            
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string LocalBinPath(string fileName = null)
        {
            return Path.Combine(_s_binFolderPath, fileName ?? "");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string LocalBinPath(string subFolder, string fileName)
        {
            return Path.Combine(Path.Combine(_s_binFolderPath, subFolder), fileName);
        }
    }
}

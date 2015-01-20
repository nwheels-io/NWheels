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
        private static readonly string s_BinFolderPath = Path.GetDirectoryName(typeof(PathUtility).Assembly.Location);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string LocalBinPath(string fileName = null)
        {
            return Path.Combine(s_BinFolderPath, fileName ?? "");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string LocalBinPath(string subFolder, string fileName)
        {
            return Path.Combine(Path.Combine(s_BinFolderPath, subFolder), fileName);
        }
    }
}

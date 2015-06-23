using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static string HostBinPath(string fileName = null)
        {
            return Path.Combine(_s_binFolderPath, fileName ?? "");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string HostBinPath(string subFolder, string fileName)
        {
            return Path.Combine(Path.Combine(_s_binFolderPath, subFolder), fileName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ModuleBinPath(Assembly module, string fileName = null)
        {
            var moduleDirectory = Path.GetDirectoryName(module.Location);
            return Path.Combine(moduleDirectory, fileName ?? "");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ModuleBinPath(Assembly module, string subFolder, string fileName)
        {
            var moduleDirectory = Path.GetDirectoryName(module.Location);
            return Path.Combine(Path.Combine(moduleDirectory, subFolder), fileName);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetAbsolutePath(string relativePath, string relativeTo, bool isRelativeToFile = false)
        {
            var originDirectory = (isRelativeToFile ? Path.GetDirectoryName(relativeTo) : relativeTo);
            var combinedAbsolutePath = Path.Combine(originDirectory, relativePath);
            var cleanAbsolutePath = Path.GetFullPath(new Uri(combinedAbsolutePath).LocalPath).Replace("/", "\\");

            return cleanAbsolutePath;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetRelativePath(string absolutePath, string relativeTo)
        {
            var relativePath = new Uri(relativeTo).MakeRelativeUri(new Uri(absolutePath)).ToString().Replace("/", "\\");
            return relativePath;
        }
    }
}

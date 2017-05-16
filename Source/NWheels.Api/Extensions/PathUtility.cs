using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NWheels.Extensions
{
    public static class PathUtility
    {
        private static readonly string _s_binaryFolderPath = 
            Path.GetDirectoryName(typeof(PathUtility).GetTypeInfo().Assembly.Location);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ExpandPathFromBinary(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return Path.Combine(_s_binaryFolderPath, path.ToPathString());
            }

            return _s_binaryFolderPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ExpandPathFromBinary(params string[] pathParts)
        {
            if (pathParts != null)
            {
                var allPathParts = new string[pathParts.Length + 1];
                allPathParts[0] = _s_binaryFolderPath;
                
                for (int i = 0 ; i < pathParts.Length; i++)
                {
                    allPathParts[i + 1] = pathParts[i].ToPathString();
                }

                return Path.Combine(allPathParts);
            }

            return _s_binaryFolderPath;
        }
    }
}

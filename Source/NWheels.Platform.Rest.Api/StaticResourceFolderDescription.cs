using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public class StaticResourceFolderDescription
    {
        public StaticResourceFolderDescription(string folderUriPath, string folderLocalPath, string defaultFileName)
        {
            this.FolderUriPath = folderUriPath;
            this.FolderLocalPath = folderLocalPath;
            this.DefaultFileName = defaultFileName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FolderUriPath { get; }
        public string FolderLocalPath { get; }
        public string DefaultFileName { get; }
    }
}

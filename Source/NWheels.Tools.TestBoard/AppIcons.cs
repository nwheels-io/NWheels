using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NWheels.Tools.TestBoard
{
    public static class AppIcons
    {
        private static readonly ConcurrentDictionary<string, ImageSource> _s_imageCache = 
            new ConcurrentDictionary<string, ImageSource>(StringComparer.InvariantCultureIgnoreCase);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource Get(string pngImageName)
        {
            return _s_imageCache.GetOrAdd(pngImageName, valueFactory: CreateImageSource);            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconApplication
        {
            get
            {
                return Get("AppExplorerIconApplication");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconConfigFile
        {
            get
            {
                return Get("AppExplorerIconConfigFile");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconConfigurationEditor
        {
            get
            {
                return Get("AppExplorerIconConfigurationEditor");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconConfigurationFolder
        {
            get
            {
                return Get("AppExplorerIconConfigurationFolder");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconEnvironment
        {
            get
            {
                return Get("AppExplorerIconEnvironment");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconLogFolder
        {
            get
            {
                return Get("AppExplorerIconLogFolder");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconNode
        {
            get
            {
                return Get("AppExplorerIconNode");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconNodeInstance
        {
            get
            {
                return Get("AppExplorerIconNodeInstance");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconPlainLog
        {
            get
            {
                return Get("AppExplorerIconPlainLog");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ImageSource AppExplorerIconThreadLog
        {
            get
            {
                return Get("AppExplorerIconThreadLog");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ImageSource CreateImageSource(string pngImageName)
        {
            var packUri = "pack://application:,,,/ntest;component/Resources/" + pngImageName + ".png";
            return (ImageSource)(new ImageSourceConverter().ConvertFromString(packUri));
        }
    }
}

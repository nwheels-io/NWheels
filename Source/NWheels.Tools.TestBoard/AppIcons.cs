using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NWheels.Tools.TestBoard
{
    public static class AppIcons
    {
        public static ImageSource Get(string pngImageName)
        {
            string packUri = "pack://application:,,,/ntest;component/Resources/" + pngImageName + ".png";
            return new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;            
        }
    }
}

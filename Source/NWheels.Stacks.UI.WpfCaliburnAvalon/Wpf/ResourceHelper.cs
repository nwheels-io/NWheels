using System;
using System.Windows.Media.Imaging;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Wpf
{
    public static class ResourceHelper
    {
        public static BitmapImage LoadBitmap(string resourceName)
        {
            var bitmap = new BitmapImage();
            
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,/Resources/" + resourceName);
            bitmap.EndInit();

            return bitmap;
        }
    }
}

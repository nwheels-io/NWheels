using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IScreen : IUIElement
    {
        string Icon { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class ScreenFluentApi
    {
        public static T Icon<T>(this T screen, string icon) where T : IScreen
        {
            screen.Icon = icon;
            return screen;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Title<T>(this T screen, string title) where T : IScreen
        {
            screen.Icon = title;
            return screen;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T SubTitle<T>(this T screen, string subTitle) where T : IScreen
        {
            screen.SubTitle = subTitle;
            return screen;
        }
    }
}

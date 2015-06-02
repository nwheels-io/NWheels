using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IUIApplication
    {
        string Icon { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        string Copyright { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UIApplicationExtensions
    {
        public static T Icon<T>(this T app, string icon) where T : IUIApplication
        {
            app.Icon = icon;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Title<T>(this T app, string title) where T : IUIApplication
        {
            app.Icon = title;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T SubTitle<T>(this T app, string subTitle) where T : IUIApplication
        {
            app.SubTitle = subTitle;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Copyright<T>(this T app, string copyright) where T : IUIApplication
        {
            app.Copyright = copyright;
            return app;
        }

    }
}

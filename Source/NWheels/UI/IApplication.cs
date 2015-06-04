using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IApplication : IUIElement
    {
        string Icon { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        string Copyright { get; set; }
        IScreen InitialScreen { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UIApplicationExtensions
    {
        public static T AppIcon<T>(this T app, string icon) where T : IApplication
        {
            app.Icon = icon;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T AppTitle<T>(this T app, string title) where T : IApplication
        {
            app.Icon = title;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T AppSubTitle<T>(this T app, string subTitle) where T : IApplication
        {
            app.SubTitle = subTitle;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T AppCopyright<T>(this T app, string copyright) where T : IApplication
        {
            app.Copyright = copyright;
            return app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T InitialScreen<T>(this T app, Func<T, IScreen> screenSelector) where T : IApplication
        {
            app.InitialScreen = screenSelector(app);
            return app;
        }
    }
}

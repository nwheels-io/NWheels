using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI;
using NWheels.UI.Elements;

namespace NWheels.Samples.BloggingPlatform.Apps
{
    internal class BlogApp : IUiApplication
    {
        public void BuildApplication(IUiApplicationBuilder app)
        {
            app.Id = "Blog";
            app.AddScreen("Front", FrontScreen);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FrontScreen(IUiScreenBuilder<Unbound.Model, Unbound.State> screen)
        {
            
        }
    }
}

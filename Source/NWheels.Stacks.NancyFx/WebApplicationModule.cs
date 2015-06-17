using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Json;
using Nancy.Responses;
using NWheels.UI;
using NWheels.UI.Core;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationModule : NancyModule
    {
        private readonly ApplicationDescription _application;
        private readonly string _contentRootPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationModule(ApplicationDescription application)
        {
            _application = application;

            base.Get["/"] = parameters => View[GetSkinViewPath(_application.InitialScreen.IdName)];
            base.Get["/meta.json"] = parameters => GetApplicationMetadata();//Response.AsJson<ScreenDescription>(GetInitialScreen());// _application.Screens.Find(s => s.IdName == _application.InitialScreenIdName));

            _contentRootPath = Path.Combine(Path.GetDirectoryName(_application.GetType().Assembly.Location), _application.IdName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetApplicationMetadata()
        {
            var serializer = new MetadataJsonSerializer();
            return new JsonResponse<ApplicationDescription>(_application, serializer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ScreenDescription GetInitialScreen()
        {
            var screen = _application.Screens.Find(s => s.IdName == _application.InitialScreen.IdName);
            return screen;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetSkinViewPath(string viewName)
        {
            return ("Content/Skin.Default/" + viewName + ".html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContentRootPath
        {
            get { return _contentRootPath; }
        }
    }
}

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
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationModule : NancyModule
    {
        private readonly UidlDocument _uidl;
        private readonly UidlApplication _application;
        private readonly string _contentRootPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationModule(UidlDocument uidl)
        {
            _uidl = uidl;
            _application = uidl.Applications[0];

            base.Get["/"] = parameters => View["Skin.Default/index.html"];
            base.Get["/uidl.json"] = parameters => GetUidlResponse();

            _contentRootPath = PathUtility.LocalBinPath(_application.IdName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetUidlResponse()
        {
            var serializer = new MetadataJsonSerializer();
            return new JsonResponse<UidlDocument>(_uidl, serializer);
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly Assembly _applicationAssembly;
        private readonly UidlApplication _application;
        private readonly string _contentRootPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationModule(UidlDocument uidl, Assembly applicationAssembly)
        {
            _uidl = uidl;
            _applicationAssembly = applicationAssembly;
            _application = uidl.Applications[0];

            base.Get["/"] = parameters => View["index.html"];
            base.Get["/uidl.json"] = parameters => GetUidlResponse();

            _contentRootPath = PathUtility.ModuleBinPath(_applicationAssembly, _application.IdName) + "\\Skin.Default";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetUidlResponse()
        {
            var serializer = new MetadataJsonSerializer();
            return new JsonResponse<UidlDocument>(_uidl, serializer);
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public string GetSkinViewPath(string viewName)
        //{
        //    return ("Content/Skin.Default/" + viewName + ".html");
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContentRootPath
        {
            get { return _contentRootPath; }
        }
    }
}

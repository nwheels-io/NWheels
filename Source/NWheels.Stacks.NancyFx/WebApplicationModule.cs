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
        private readonly UidlApplication _application;
        private readonly Dictionary<Type, object> _domainApis;
        private readonly Dictionary<string, Type> _domainApiContractTypeByName;
        private readonly string _contentRootPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationModule(
            UidlDocument uidl, 
            UidlApplication application, 
            Dictionary<Type, object> domainApis)
        {
            _uidl = uidl;
            _application = application;
            _domainApis = domainApis;

            base.Get["/"] = parameters => View["index.html"];
            base.Get["/uidl.json"] = parameters => GetUidl();
            base.Post["/api/{contract}/{operation}"] = parameters => ExecuteDomainApi(parameters);

            _domainApiContractTypeByName = _domainApis.Keys.ToDictionary(type => type.Name, type => type);
            _contentRootPath = PathUtility.ModuleBinPath(_application.GetType().Assembly, _application.IdName) + "\\Skin.Default";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetUidl()
        {
            var serializer = new MetadataJsonSerializer();
            return new JsonResponse<UidlDocument>(_uidl, serializer);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response ExecuteDomainApi(dynamic parameters)
        {
            var contractType = _domainApiContractTypeByName[parameters.contract];
            return Response.AsJson(new { Success = true });
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

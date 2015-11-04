using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace NWheels.Stacks.AspNet
{
    public class UidlApplicationStaticContentHandler : IHttpHandler, IRouteHandler
    {
        private readonly IWebModuleContext _webModuleContext;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlApplicationStaticContentHandler(IWebModuleContext context)
        {
            _webModuleContext = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IHttpHandler

        public void ProcessRequest(HttpContext context)
        {
            var requestPath = context.Request.Path.ToLower();
            string filePath = null;

            if ( requestPath.StartsWith("base/") )
            {
                filePath = Path.Combine(_webModuleContext.ContentRootPath, _webModuleContext.BaseSubFolderName, requestPath.Substring(5).Replace("/", "\\"));
            }
            else if ( requestPath.StartsWith("skin/") )
            {
                filePath = Path.Combine(_webModuleContext.ContentRootPath, _webModuleContext.SkinSubFolderName, requestPath.Substring(5).Replace("/", "\\"));
            }

            if ( filePath != null && File.Exists(filePath) )
            {
                context.Response.TransmitFile(filePath);
                context.Response.ContentType = MimeMapping.GetMimeMapping(filePath);
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsReusable
        {
            get { return true; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRouteHandler

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        #endregion
    }
}

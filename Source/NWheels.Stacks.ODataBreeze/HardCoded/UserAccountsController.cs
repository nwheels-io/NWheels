#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Breeze.ContextProvider;
using Breeze.WebApi2;
using Newtonsoft.Json.Linq;
using NWheels.DataObjects;
using NWheels.Domains.Security;

namespace NWheels.Stacks.ODataBreeze.HardCoded
{
    [BreezeController]
    [EnableCors("*", "*", "*")]
    public class UserAccountsController : ApiController
    {
        private readonly UserAccountsContextProvider _contextProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountsController(IFramework framework, ITypeMetadataCache metadataCache)
        {
            _contextProvider = new UserAccountsContextProvider(framework, metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public string Metadata()
        {
            var metadataJsonString = _contextProvider.Metadata();
            //var metadataJsonString = File.ReadAllText(@"C:\Temp\metadata.json");
            return metadataJsonString;
        }


        //// ~/breeze/todos/Metadata 
        //[HttpGet]
        //public HttpResponseMessage Metadata()
        //{
        //    var metadataJsonString = File.ReadAllText(@"C:\Temp\metadata.json");

        //    var response = new HttpResponseMessage() {
        //        Content = new StringContent(metadataJsonString, Encoding.UTF8, "application/json")
        //    };

        //    return response;

        //    //var metadataJsonString = File.ReadAllText(@"C:\Temp\metadata.json");
        //    ////var metadataJsonString = _contextProvider.Metadata();
        //    //return metadataJsonString;
        //}

        //[HttpGet]
        //public HttpResponseMessage UserAccount()
        //{
        //    var dataJsonString = File.ReadAllText(@"C:\Temp\data.json");
            
        //    var response = new HttpResponseMessage() {
        //        Content = new StringContent(dataJsonString, Encoding.UTF8, "application/json")
        //    };

        //    return response;
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //// ~/breeze/todos/Metadata 
        //[HttpGet]
        //public HttpResponseMessage EdmxMetadata()
        //{
        //    var metadataXmlString = ReplaceEdmxNamespaces(_contextProvider.GetRepositoryMetadataString(fullEdmx: true));
        //    //var metadataXmlString = File.ReadAllText(@"C:\Temp\edmx.xml");

        //    var response = new HttpResponseMessage() {
        //        Content = new StringContent(metadataXmlString, Encoding.UTF8, "application/xml")
        //    };

        //    response.Headers.Add("DataServiceVersion", "3.0");
        //    return response;
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IUserAccountEntity> UserAccount()
        {
            return _contextProvider.QuerySource.AllUsers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IUserRoleEntity> UserRole()
        {
            return _contextProvider.QuerySource.UserRoles;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IOperationPermissionEntity> OperationPermission()
        {
            return _contextProvider.QuerySource.OperationPermissions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IEntityAccessRuleEntity> EntityAccessRule()
        {
            return _contextProvider.QuerySource.EntityAccessRules;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _contextProvider.SaveChanges(saveBundle);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private string ReplaceEdmxNamespaces(string xml)
        {
            const string from1 = "<edmx:Edmx Version=\"3.0\" xmlns:edmx=\"http://schemas.microsoft.com/ado/2009/11/edmx\">";
            const string to1 = "<edmx:Edmx Version=\"1.0\" xmlns:edmx=\"http://schemas.microsoft.com/ado/2007/06/edmx\">";

            const string from2 = "<edmx:DataServices>";
            const string to2 = "<edmx:DataServices m:DataServiceVersion=\"3.0\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\">";

            return xml.Replace(from1, to1).Replace(from2, to2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQPad.Extensibility.DataContext;
using System.Reflection;
using Microsoft.OData.Client;

namespace LinqPadODataV4Driver
{
    public class ODataV4Driver : DynamicDataContextDriver
    {
        public override List<ExplorerItem> GetSchemaAndBuildAssembly(
            IConnectionInfo cxInfo, 
            AssemblyName assemblyToBuild, 
            ref string nameSpace, 
            ref string typeName)
        {
            var builder = new SchemaBuilder();
            return builder.GetSchemaAndBuildAssembly(
                new ConnectionProperties(cxInfo),
                assemblyToBuild,
                ref nameSpace,
                ref typeName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            // We need to pass the chosen URI into the DataServiceContext's constructor:
            return new[] { new ParameterDescriptor("serviceRoot", "System.Uri") };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            // We need to pass the chosen URI into the DataServiceContext's constructor:
            return new object[] { new Uri(new ConnectionProperties(cxInfo).Uri) };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            // This method gets called after a DataServiceContext has been instantiated. It gives us a chance to
            // perform further initialization work.
            //
            // And as it happens, we have an interesting problem to solve! The typed data service context class
            // that Astoria's EntityClassGenerator generates handles the ResolveType delegate as follows:
            //
            //   return this.GetType().Assembly.GetType (string.Concat ("<namespace>", typeName.Substring (19)), true);
            //
            // Because LINQPad subclasses the typed data context when generating a query, GetType().Assembly returns
            // the assembly of the user query rather than the typed data context! To work around this, we must take
            // over the ResolveType delegate and resolve using the context's base type instead:

            var dsContext = (DataServiceContext)context;
            var typedDataServiceContextType = context.GetType().BaseType;

            dsContext.ResolveType = name => typedDataServiceContextType.Assembly.GetType
                (typedDataServiceContextType.Namespace + "." + name.Split('.').Last());

            // The next step is to feed any supplied credentials into the Astoria service.
            // (This could be enhanced to support other authentication modes, too).
            //var props = new AstoriaProperties(cxInfo);
            //dsContext.Credentials = props.GetCredentials();

            // Finally, we handle the SendingRequest event so that it writes the request text to the SQL translation window:
            dsContext.SendingRequest2 += (sender, e) => executionManager.SqlTranslationWriter.WriteLine(e.RequestMessage.Url);
        }		

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Obsolete]
        public override IEnumerable<string> GetAssembliesToAdd()
        {
            var binPath = Path.GetDirectoryName(typeof (ODataV4Driver).Assembly.Location);

            // We need the following assembly for compiliation and autocompletion:
            var assemblyNames = new[] {
                "Microsoft.OData.Client", 
                "Microsoft.OData.Core", 
                "Microsoft.OData.Edm", 
                "System.Web.OData", 
                "System.Web.Http", 
                "System.Net.Http", 
                "Newtonsoft.Json", 
                "Microsoft.Spatial"
            };

            return assemblyNames.Select(name => Path.Combine(binPath, name) + ".dll").ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Obsolete]
        public override IEnumerable<string> GetNamespacesToAdd()
        {
            // Import the commonly used namespaces as a courtesy to the user:
            return new[] { "Microsoft.OData.Client" };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool AreRepositoriesEquivalent(IConnectionInfo r1, IConnectionInfo r2)
        {
            // Two repositories point to the same endpoint if their URIs are the same.
            return object.Equals(r1.DriverData.Element("Uri"), r2.DriverData.Element("Uri"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            if ( isNewConnection )
            {
                new ConnectionProperties(cxInfo).Uri = "http://your-domain/your-app/odata";
            }

            bool? result = new ConnectionDialog(cxInfo).ShowDialog();
            return (result == true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Author
        {
            get
            {
                return "Felix Berman";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return new ConnectionProperties(cxInfo).Uri;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return "OData v4 Endpoint"; }
        }
    }
}

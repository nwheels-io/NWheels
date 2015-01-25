using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;

namespace DataContextDriverDemo.Astoria
{
	/// <summary>
	/// Sample dynamic driver. This lets users connect to an ADO.NET Data Services URI, builds the
	/// type data context dynamically, and returns objects for the Schema Explorer.
	/// </summary>
	public class AstoriaDynamicDriver : DynamicDataContextDriver
	{
		public override string Name { get { return "ADO.NET Data Services 3.5 (Demo)"; } }

		public override string Author { get { return "Joe Albahari"; } }

		public override string GetConnectionDescription (IConnectionInfo cxInfo)
		{
			// The URI of the service best describes the connection:
			return new AstoriaProperties (cxInfo).Uri;
		}

		public override ParameterDescriptor [] GetContextConstructorParameters (IConnectionInfo cxInfo)
		{
			// We need to pass the chosen URI into the DataServiceContext's constructor:
			return new [] { new ParameterDescriptor ("serviceRoot", "System.Uri") };
		}

		public override object [] GetContextConstructorArguments (IConnectionInfo cxInfo)
		{
			// We need to pass the chosen URI into the DataServiceContext's constructor:
			return new object [] { new Uri (new AstoriaProperties (cxInfo).Uri) };
		}

		public override IEnumerable<string> GetAssembliesToAdd ()
		{
			// We need the following assembly for compiliation and autocompletion:
			return new [] { "System.Data.Services.Client.dll" };
		}

		public override IEnumerable<string> GetNamespacesToAdd ()
		{
			// Import the commonly used namespaces as a courtesy to the user:
			return new [] { "System.Data.Services.Client" };
		}

		public override bool AreRepositoriesEquivalent (IConnectionInfo r1, IConnectionInfo r2)
		{
			// Two repositories point to the same endpoint if their URIs are the same.
			return object.Equals (r1.DriverData.Element ("Uri"), r2.DriverData.Element ("Uri"));
		}

		public override void InitializeContext (IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
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
			var typedDataServiceContextType = context.GetType ().BaseType;

			dsContext.ResolveType = name => typedDataServiceContextType.Assembly.GetType
				(typedDataServiceContextType.Namespace + "." + name.Split ('.').Last ());

			// The next step is to feed any supplied credentials into the Astoria service.
			// (This could be enhanced to support other authentication modes, too).
			var props = new AstoriaProperties (cxInfo);
			dsContext.Credentials = props.GetCredentials ();

			// Finally, we handle the SendingRequest event so that it writes the request text to the SQL translation window:
			dsContext.SendingRequest += (sender, e) => executionManager.SqlTranslationWriter.WriteLine (e.Request.RequestUri);
		}		

		public override bool ShowConnectionDialog (IConnectionInfo cxInfo, bool isNewConnection)
		{
			// Populate the default URI with a demo value:
			if (isNewConnection) new AstoriaProperties (cxInfo).Uri = "http://ogdi.cloudapp.net/v1/dc";

			bool? result = new ConnectionDialog (cxInfo).ShowDialog ();
			return result == true;
		}

		public override List<ExplorerItem> GetSchemaAndBuildAssembly (IConnectionInfo cxInfo, AssemblyName assemblyToBuild,
			ref string nameSpace, ref string typeName)
		{ 
			return SchemaBuilder.GetSchemaAndBuildAssembly (
				new AstoriaProperties (cxInfo),
				assemblyToBuild, 
				ref nameSpace,
				ref typeName);
		}		
	}
}

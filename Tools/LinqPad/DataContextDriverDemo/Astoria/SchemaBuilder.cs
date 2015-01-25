using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Services.Client;
using System.Data.Services.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;

namespace DataContextDriverDemo.Astoria
{
	class SchemaBuilder
	{
		internal static List<ExplorerItem> GetSchemaAndBuildAssembly (AstoriaProperties props, AssemblyName name,
			ref string nameSpace, ref string typeName)
		{
			// Read the EDM schema into an XDocument:
			XDocument data;
			using (XmlReader reader = GetSchemaReader (props))
				data = XDocument.Load (reader);

			// Generate the code using the ADO.NET Data Services classes:
			string code;
			using (XmlReader reader = data.CreateReader ())
				code = GenerateCode (reader, nameSpace);

			// Compile the code into the assembly, using the assembly name provided:
			BuildAssembly (code, name);

			// Use the schema to populate the Schema Explorer:
			List<ExplorerItem> schema = GetSchema (data, out typeName);

			return schema;
		}

		static string GenerateCode (XmlReader reader, string nameSpace)
		{
			// We're using EntityClassGenerator to generate C# code, which we'll compile in
			// the BuildAssembly method. (Another solution is to generate IL directly via Reflection.Emit.)

			var generator = new EntityClassGenerator (LanguageOption.GenerateCSharpCode);
			var writer = new StringWriter ();
			
			IList<EdmSchemaError> errors = generator.GenerateCode (reader, writer, nameSpace);

			// If we encounter errors, we can just throw an exception.
			if (errors.Count > 0) 
				throw new Exception ("Bad schema: " + errors [0].Message + " (line " + errors [0].Line + ")");

			return writer.ToString ();
		}

		static XmlReader GetSchemaReader (AstoriaProperties props)
		{
			var uri = new DataServiceContext (new Uri (props.Uri)).GetMetadataUri ();

			var settings = new XmlReaderSettings ();
			XmlResolver resolver = new XmlUrlResolver ();

			// If credentials have been supplied, feed these in as we're querying the schema:
			resolver.Credentials = props.GetCredentials ();

			settings.XmlResolver = resolver;

			return XmlReader.Create (uri.ToString (), settings);
		}

		static void BuildAssembly (string code, AssemblyName name)
		{
			// Use the CSharpCodeProvider to compile the generated code:
			CompilerResults results;
			using (var codeProvider = new CSharpCodeProvider (new Dictionary<string, string> () { { "CompilerVersion", "v3.5" } }))
			{
				var options = new CompilerParameters (
					"System.dll System.Core.dll System.Xml.dll System.Data.Services.Client.dll".Split (),
					name.CodeBase,
					true);
				results = codeProvider.CompileAssemblyFromSource (options, code);
			}
			if (results.Errors.Count > 0)
				throw new Exception 
					("Cannot compile typed context: " + results.Errors [0].ErrorText + " (line " + results.Errors [0].Line + ")");
		}

		static List<ExplorerItem> GetSchema (XDocument data, out string typeName)
		{
			// We're going to populate the Schema Explorer using the EDM metadata rather than reflecting over
			// the typed data context. This gives us more flexibility in properly representing associations.
			// For instance, we can correctly display 1:1 and many:many relationships based on the EDM.

			XNamespace edm = "http://schemas.microsoft.com/ado/2006/04/edm";
			XNamespace edmx = "http://schemas.microsoft.com/ado/2007/06/edmx";
			XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

			var edmxNode = data.Root.Element (edmx + "DataServices");

			// Read the associations into a dictionary for easier processing later.
			//   key = schema namespace + name.
			//   value = dictionary of associations (role vs multiplicity).

			Dictionary<string, Dictionary<string, string>> associations =
			(
				from schema in edmxNode.Elements (edm + "Schema")
				let schemaNS = schema.Attribute ("Namespace").Value
				from association in schema.Elements (edm + "Association")
				select new { schemaNS, association }
			)
			.ToDictionary
			(
				a => a.schemaNS + "." + a.association.Attribute ("Name").Value,
				a => a.association.Elements (edm + "End").ToDictionary
				(
					e => e.Attribute ("Role").Value,
					e => e.Attribute ("Multiplicity").Value
				)
			);

			var entities =
			(
				from schema in edmxNode.Elements (edm + "Schema")
				let schemaNS = schema.Attribute ("Namespace").Value
				from entityType in schema.Elements (edm + "EntityType")
				let keys = entityType.Element (edm + "Key") == null
					? new HashSet<string> ()
					: new HashSet<string>		// This hashset will let us know later whether each property is a key.
					(
						entityType.Element (edm + "Key").Elements (edm + "PropertyRef").Select (e => e.Attribute ("Name").Value)
					)
				select new
				{
					SchemaNamespace = schemaNS,
					EntityTypeName = entityType.Attribute ("Name").Value,
					Children =
					(
						from prop in entityType.Elements (edm + "Property")
						let propName = prop.Attribute ("Name").Value
						select new ExplorerItem (
							propName + " (" + prop.Attribute ("Type").Value.Split ('.').Last () + ")",
							ExplorerItemKind.Property, 
							keys.Contains (propName) ? ExplorerIcon.Key : ExplorerIcon.Column)
						{							
							DragText = propName,
							Kind = ExplorerItemKind.Property
						}
					).ToList (),
					AssociatedChildren =
					(
						from prop in entityType.Elements (edm + "NavigationProperty")
						let propertyName = prop.Attribute ("Name").Value
						let associationName = prop.Attribute ("Relationship").Value
						let association = associations [associationName]
						let fromMultiplicity = association [prop.Attribute ("FromRole").Value]
						let toRole = prop.Attribute ("ToRole").Value
						let toMultiplicity = association [toRole]
						let icon = GetMultiplicityIcon (fromMultiplicity, toMultiplicity)
						let kind = icon == ExplorerIcon.OneToMany || icon == ExplorerIcon.ManyToMany
							? ExplorerItemKind.CollectionLink
							: ExplorerItemKind.ReferenceLink
						orderby icon, propertyName
						select new
						{
							TargetTypeName = schemaNS + "." + toRole,
							UserMemberInfo = new ExplorerItem (propertyName, kind, icon)
							{
								DragText = propertyName,
								ToolTipText = toRole,
							}
						}
					).ToList ()
				}
			).ToList ();

			var entitiesDictionary = entities.ToDictionary (e => e.SchemaNamespace + "." + e.EntityTypeName);

			// Find the EntityContainer - this contains the EntitySets in our schema. If there are multiple containers,
			// pick the one marked with 'IsDefaultEntityContainer'.

			XElement container = edmxNode
				.Elements (edm + "Schema")
				.Elements (edm + "EntityContainer")
				.OrderBy (c => c.Attribute (m + "IsDefaultEntityContainer") == null)
				.First ();

			typeName = container.Attribute ("Name").Value;

			// Each EntitySet corresponds to a property on the typed data context, such as Customers or Orders.
			// The schema for the Customer or Order object comes from the entities that we populated previously,
			// which have now been loaded into a dictionary keyed by their type name.

			var entitySets =
			(
				from entitySet in container.Elements (edm + "EntitySet")
				let entityType = entitiesDictionary [entitySet.Attribute ("EntityType").Value]
				select new
				{
					FullTypeName = entityType.SchemaNamespace + "." + entityType.EntityTypeName,
					UserMemberInfo = new ExplorerItem (entitySet.Attribute ("Name").Value, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
					{
						DragText = entitySet.Attribute ("Name").Value,
						ToolTipText = entityType.EntityTypeName,
						IsEnumerable = true,

						// The children of this node are the entity's simple properties + its associations.
						Children = entityType.Children
							.Concat (entityType.AssociatedChildren.Select (ac => ac.UserMemberInfo))
							.ToList ()
					}
				}
			).ToList ();

			// The final step is to populate the HyperlinkTargets for the association properties.

			var entitySetDictionary = entitySets.ToDictionary (e => e.FullTypeName, e => e.UserMemberInfo);

			foreach (var entity in entities)
				foreach (var child in entity.AssociatedChildren)
					if (entitySetDictionary.ContainsKey (child.TargetTypeName))
						child.UserMemberInfo.HyperlinkTarget = entitySetDictionary [child.TargetTypeName];

			return entitySets.OrderBy (es => es.UserMemberInfo.Text).Select (es => es.UserMemberInfo).ToList ();
		}

		static ExplorerIcon GetMultiplicityIcon (string fromMultiplicity, string toMultiplicity)
		{
			return
				fromMultiplicity.Contains ("1") && toMultiplicity.Contains ("1") ? ExplorerIcon.OneToOne :
				fromMultiplicity == "*" && toMultiplicity == "*" ? ExplorerIcon.ManyToMany :
				fromMultiplicity == "*" ? ExplorerIcon.ManyToOne :
				ExplorerIcon.OneToMany;
		}
	}
}

using System.IO;
using System.Reflection;
using Hapil;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace LinqPadODataV4Driver
{
    public class SchemaBuilder
    {
        internal List<ExplorerItem> GetSchemaAndBuildAssembly(
            ConnectionProperties connectionProperties, 
            AssemblyName assemblyToBuild, 
            ref string nameSpace, 
            ref string typeName)
        {
            File.WriteAllText(@"D:\LinqPadODataV4Driver.log", string.Format(
                "GetSchemaAndBuildAssembly(assemblyToBuild={0}, nameSpace={1}, typeName={2}", 
                assemblyToBuild.CodeBase, nameSpace, typeName));

            var simpleName = Path.GetFileNameWithoutExtension(assemblyToBuild.CodeBase);
            var module = new DynamicModule(simpleName, allowSave: true, saveDirectory: Path.GetDirectoryName(assemblyToBuild.CodeBase));
            var model = DynamicDataServiceContextBase.LoadModelFromService(new Uri(connectionProperties.Uri + "/$metadata"));
            var factory = new DynamicDataServiceContextFactory(module, model);

            var generatedType = factory.BuildDynamicDataServiceContext();

            nameSpace = generatedType.Namespace;
            typeName = generatedType.Name;

            module.SaveAssembly();

            return BuildSchema(model);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<ExplorerItem> BuildSchema(IEdmModel model)
        {
            List<ExplorerItem> rootItems = new List<ExplorerItem>();
            
            var allEntityTypes = model.SchemaElements.OfType<IEdmEntityType>().ToArray();

            foreach ( var entityType in allEntityTypes )
            {
                var entityItem = new ExplorerItem(entityType.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table);

                foreach (var property in entityType.Properties())
                {
                    var propertyItem = new ExplorerItem(property.Name, ExplorerItemKind.Property, ExplorerIcon.Column);

                    if ( entityItem.Children == null )
                    {
                        entityItem.Children = new List<ExplorerItem>();
                    }

                    entityItem.Children.Add(propertyItem);
                }

                rootItems.Add(entityItem);
                Console.WriteLine("{0}{{{1}}}", entityType.Name, string.Join(",", entityType.Properties().Select(p => p.Name).ToArray()));

            }

            return rootItems;
        }
    }
}

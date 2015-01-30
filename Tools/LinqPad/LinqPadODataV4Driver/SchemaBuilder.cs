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
            var simpleName = Path.GetFileNameWithoutExtension(assemblyToBuild.CodeBase);
            var module = new DynamicModule(simpleName, allowSave: true, saveDirectory: Path.GetDirectoryName(assemblyToBuild.CodeBase));
            var model = ODataClientContextBase.LoadModelFromService(new Uri(connectionProperties.Uri + "/$metadata"));
            var factory = new ODataClientContextFactory(module);

            var generatedType = factory.ImplementClientContext(model);

            nameSpace = generatedType.Namespace;
            typeName = generatedType.Name;

            module.SaveAssembly();

            return BuildSchema(model);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<ExplorerItem> BuildSchema(IEdmModel model)
        {
            var rootItems = new List<ExplorerItem>();
            var entityItemsByEntityType = new Dictionary<IEdmEntityType, ExplorerItem>();
            
            var allEntityTypes = model.SchemaElements.OfType<IEdmEntityType>().ToArray();

            foreach ( var entityType in allEntityTypes )
            {
                var entityItem = new ExplorerItem(entityType.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table);
                entityItem.ToolTipText = entityType.FullTypeName();
                entityItem.Children = new List<ExplorerItem>();
                entityItemsByEntityType.Add(entityType, entityItem);

                foreach ( var property in entityType.Properties() )
                {
                    string itemText;
                    string itemTooltip;
                    ExplorerItemKind itemKind;
                    ExplorerIcon itemIcon;
                    SetPropertyItemStyle(entityType, property, out itemText, out itemTooltip, out itemKind, out itemIcon);

                    var propertyItem = new ExplorerItem(itemText, itemKind, itemIcon);
                    var navigationProperty = (property as IEdmNavigationProperty);

                    if ( navigationProperty != null )
                    {
                        propertyItem.Tag = navigationProperty.ToEntityType();
                    }

                    propertyItem.ToolTipText = itemTooltip;
                    entityItem.Children.Add(propertyItem);
                }

                rootItems.Add(entityItem);
            }

            FixupNavigationLinks(rootItems, entityItemsByEntityType);
            return rootItems;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FixupNavigationLinks(List<ExplorerItem> entityItems, Dictionary<IEdmEntityType, ExplorerItem> entityItemsByEntityType)
        {
            foreach (var entityItem in entityItems)
            {
                foreach (var propertyItem in entityItem.Children)
                {
                    var linkedEntityType = (propertyItem.Tag as IEdmEntityType);

                    if (linkedEntityType != null)
                    {
                        ExplorerItem linkedItem;

                        if (entityItemsByEntityType.TryGetValue(linkedEntityType, out linkedItem))
                        {
                            propertyItem.HyperlinkTarget = linkedItem;
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetPropertyItemStyle(
            IEdmEntityType entityType, 
            IEdmProperty property, 
            out string itemText,
            out string tooltipText,
            out ExplorerItemKind itemKind, 
            out ExplorerIcon itemIcon)
        {
            var navigationProperty = (property as IEdmNavigationProperty);
            var structuralProperty = (property as IEdmStructuralProperty);
            var isCollection = property.Type.IsCollection();

            if ( navigationProperty != null )
            {
                itemText = property.Name;
                tooltipText = property.Type.Definition.FullTypeName();
                itemKind = (isCollection ? ExplorerItemKind.CollectionLink : ExplorerItemKind.ReferenceLink);

                switch (navigationProperty.TargetMultiplicity())
                {
                    case EdmMultiplicity.One:
                        itemIcon = (isCollection ? ExplorerIcon.ManyToOne : ExplorerIcon.OneToOne);
                        break;
                    case EdmMultiplicity.Many:
                        itemIcon = (isCollection ? ExplorerIcon.ManyToMany : ExplorerIcon.OneToMany);
                        break;
                    default:
                        itemIcon = ExplorerIcon.Column;
                        break;
                }
            }
            else if (structuralProperty != null && entityType.DeclaredKey.Contains(structuralProperty))
            {
                itemText = string.Format("{0} : {1}", property.Name, property.Type.Definition.FullTypeName());
                tooltipText = "Contained in entity key";
                itemKind = ExplorerItemKind.Property;
                itemIcon = ExplorerIcon.Key;
            }
            else
            {
                itemText = string.Format("{0} : {1}", property.Name, property.Type.Definition.FullTypeName());
                tooltipText = string.Empty;
                itemKind = ExplorerItemKind.Property;
                itemIcon = ExplorerIcon.Column;
            }
        }
    }
}

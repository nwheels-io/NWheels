using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Hapil;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class DataGrid : WidgetBase<DataGrid, Empty.Data, Empty.State>
    {
        public DataGrid(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            DisplayColumns = new List<GridColumn>();
            IncludedProperties = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<DataGrid, Empty.Data, Empty.State> presenter)
        {
            FindPropertiesToInclude();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FindPropertiesToInclude()
        {
            var metaType = MetadataCache.GetTypeMetadata(this.EntityName);

            foreach ( var column in DisplayColumns )
            {
                if ( column.Navigations.Length > 1 )
                {
                    for ( int i = 0 ; i < column.Navigations.Length ; i++ )
                    {
                        IPropertyMetadata metaProperty;

                        if ( metaType.TryGetPropertyByName(column.Navigations[i], out metaProperty) )
                        {
                            if ( metaProperty.Relation != null && i < column.Navigations.Length - 1 && !metaProperty.Relation.RelatedPartyType.IsEntityPart )
                            {
                                IncludedProperties.Add(column.Expression);
                            }
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        
        [DataMember]
        public List<GridColumn> DisplayColumns { get; set; }

        [DataMember]
        public List<GridColumn> DefaultDisplayColumns { get; set; }

        [DataMember]
        public List<string> IncludedProperties { get; set; }

        [DataMember]
        public bool UsePascalCase { get; set; }

        [DataMember]
        public DataGridMode Mode { get; set; }

        [DataMember]
        public bool EnablePaging { get; set; }

        [DataMember]
        public bool EnableAutonomousQuery { get; set; }

        [DataMember]
        public bool EnableDetailPane { get; set; }

        [DataMember]
        public bool EnableTotalRow { get; set; }

        [DataMember]
        public bool TotalRowOnTop { get; set; }

        [DataMember, ManuallyAssigned]
        public WidgetUidlNode DetailPaneWidget { get; set; }

        [DataMember]
        public bool DetailPaneExpanded { get; set; }

        [DataMember]
        public int[] PageSizeOptions { get; set; }

        //[DataMember]
        //public string DetailPaneStaticTemplateName { get; set; }

        //[DataMember]
        //public string DetailPaneTemplateNameProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<object> RequestPrepared { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns.Select(c => c.Title));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().ConcatIf(this.DetailPaneWidget);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            if ( DetailPaneWidget != null )
            {
                builder.BuildNodes(DetailPaneWidget);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
        public class GridColumn
        {
            public GridColumn(
                ITypeMetadata metaType, 
                LambdaExpression propertyNavigation, 
                string title = null, 
                FieldSize size = FieldSize.Medium, 
                string format = null,
                bool includeInTotal = false)
                : this(metaType, ParsePropertyNavigation(propertyNavigation), title, size, format, includeInTotal)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal GridColumn(
                ITypeMetadata metaType, 
                string[] navigations, 
                string title = null, 
                FieldSize size = FieldSize.Medium, 
                string format = null, 
                bool includeInTotal = false)
            {
                this.Navigations = navigations;
                this.Expression = string.Join(".", Navigations);

                this.Title = title ?? this.Navigations.Last();
                this.Size = size;
                this.Format = format;
                this.IncludeInTotal = includeInTotal;

                ITypeMetadata destinationMetaType;
                IPropertyMetadata destinationMetaProperty;
                FindDeclaringMetaType(metaType, out destinationMetaType, out destinationMetaProperty);

                this.DeclaringTypeName = destinationMetaType.QualifiedName;
                this.MetaProperty = destinationMetaProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string Title { get; set; }
            [DataMember]
            public FieldSize Size { get; set; }
            [DataMember]
            public string Format { get; set; }
            [DataMember]
            public string Expression { get; set; }
            [DataMember]
            public string[] Navigations { get; set; }
            [DataMember]
            public string DeclaringTypeName { get; set; }
            [DataMember]
            public bool IncludeInTotal { get; set; }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal protected IPropertyMetadata MetaProperty { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void FindDeclaringMetaType(ITypeMetadata metaType, out ITypeMetadata destinationMetaType, out IPropertyMetadata destinationMetaProperty)
            {
                destinationMetaType = metaType;

                for ( int i = 0; i < Navigations.Length - 1; i++ )
                {
                    var navigationMetaProperty = destinationMetaType.GetPropertyByName(Navigations[i]);

                    if ( i < Navigations.Length - 1 && navigationMetaProperty.Relation != null )
                    {
                        destinationMetaType = navigationMetaProperty.Relation.RelatedPartyType;
                    }
                    else
                    {
                        break;
                    }
                }

                destinationMetaProperty = destinationMetaType.GetPropertyByName(Navigations.Last());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            internal static string[] ParsePropertyNavigation(LambdaExpression propertyNavigation)
            {
                var expressionString = propertyNavigation.ToNormalizedNavigationString(convertToCamelCase: false);
                return expressionString.Split('.').Skip(1).ToArray();
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataGridDefaultRow : WidgetBase<DataGridDefaultRow, Empty.Data, Empty.State>
    {
        public DataGridDefaultRow(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<DataGridDefaultRow, Empty.Data, Empty.State> presenter)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
    public class DataGrid<TDataRow> : DataGrid
    {
        public DataGrid(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "DataGrid";
            this.TemplateName = "DataGrid";
            this.EntityName = typeof(TDataRow).Name.TrimLead("I").TrimTail("Entity");
            this.MetaType = MetadataCache.GetTypeMetadata(typeof(TDataRow));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> Column<T>(
            Expression<Func<TDataRow, T>> propertySelector, 
            string title = null, 
            FieldSize size = FieldSize.Medium,
            bool includeInTotal = false)
        {
            this.DisplayColumns.Add(new GridColumn(MetaType, propertySelector, title, size, null, includeInTotal));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> Column<TDerivedDataRow, T>(
            Expression<Func<TDerivedDataRow, T>> propertySelector, string title = null, FieldSize size = FieldSize.Medium, bool includeInTotal = false)
            where TDerivedDataRow : TDataRow
        {
            var derivedMetaType = MetadataCache.GetTypeMetadata(typeof(TDerivedDataRow));
            this.DisplayColumns.Add(new GridColumn(derivedMetaType, propertySelector, title, size, null, includeInTotal));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> ColumnWithNavigationTo<TDestinationEntity>(
            Expression<Func<TDataRow, object>> sourcePropertySelector,
            Expression<Func<TDestinationEntity, object>> destinationPropertySelector,
            string title = null,
            FieldSize size = FieldSize.Medium)
        {
            var navigations =
                GridColumn.ParsePropertyNavigation(sourcePropertySelector)
                .Concat(GridColumn.ParsePropertyNavigation(destinationPropertySelector))
                .ToArray();

            this.DisplayColumns.Add(new GridColumn(MetaType, navigations, title, size));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public DataGrid<TDataRow> UseDetailPane(string staticTemplateName, bool expandOnLoad)
        //{
        //    this.EnableDetailPane = true;
        //    this.DetailPaneWidget = null;
        //    this.DetailPaneStaticTemplateName = staticTemplateName;
        //    this.DetailPaneTemplateNameProperty = null;
        //    this.DetailPaneExpandedOnLoad = expandOnLoad;
        //    return this;
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public DataGrid<TDataRow> UseDetailPane(Expression<Func<TDataRow, string>> dynamicTemplateProperty, bool expandOnLoad)
        //{
        //    this.EnableDetailPane = true;
        //    this.DetailPaneWidget = null;
        //    this.DetailPaneStaticTemplateName = null;
        //    this.DetailPaneTemplateNameProperty = dynamicTemplateProperty.GetPropertyInfo().Name;
        //    this.DetailPaneExpandedOnLoad = expandOnLoad;
        //    return this;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> UseDetailPane(WidgetUidlNode widget, bool expanded)
        {
            this.EnableDetailPane = true;
            this.DetailPaneWidget = widget;
            //this.DetailPaneStaticTemplateName = null;
            //this.DetailPaneTemplateNameProperty = null;
            this.DetailPaneExpanded = expanded;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<IEnumerable<TDataRow>> DataReceived { get; set; }
        public UidlNotification<IEnumerable<TDataRow>> QueryCompleted { get; set; }
        public UidlNotification<IPromiseFailureInfo> QueryFailed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        internal ITypeMetadata MetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.RegisterMetaType(typeof(TDataRow));
            base.EntityName = MetadataCache.GetTypeMetadata(typeof(TDataRow)).QualifiedName;

            base.DefaultDisplayColumns = MetaType.Properties
                .Where(ShouldDisplayPropertyByDefault)
                .Select(p => new GridColumn(MetaType, MetaType.MakePropertyExpression(p), size: FieldSize.Medium))
                .OrderByDescending(c => GetColumnPropertyDefaultOrder(c.MetaProperty))
                .ToList();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldDisplayPropertyByDefault(IPropertyMetadata metaProperty)
        {
            return (
                (metaProperty.Role == PropertyRole.None && metaProperty.Kind == PropertyKind.Scalar) ||
                (metaProperty.Role == PropertyRole.Key && ShouldDisplayKeyPropertyOfType(metaProperty)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldDisplayKeyPropertyOfType(IPropertyMetadata metaProperty)
        {
            return (
                metaProperty.ClrType == typeof(string) || 
                metaProperty.ClrType.IsIntegralType() || 
                metaProperty.ClrType == typeof(DateTime) || 
                metaProperty.ClrType == typeof(TimeSpan));
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private int GetColumnPropertyDefaultOrder(IPropertyMetadata metaProperty)
        {
            var value = 0;

            if ( metaProperty.Role == PropertyRole.Key )
            {
                value += 100;
            }

            if ( metaProperty.DeclaringContract.DefaultDisplayProperties.Contains(metaProperty) )
            {
                value += 90;
            }

            if ( metaProperty.Kind == PropertyKind.Relation )
            {
                value += 80;
            }

            if ( metaProperty.DeclaringContract == MetaType )
            {
                value += 1000;
            }

            return value;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum DataGridMode
    {
        Standalone,
        Inline,
        LookupOne,
        LookupMany
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Hapil;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Globalization.Core;
using NWheels.UI.Core;
using NWheels.UI.Uidl;
using NWheels.TypeModel;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class DataGrid : WidgetBase<DataGrid, Empty.Data, Empty.State>
    {
        public DataGrid(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            DisplayColumns = new List<GridColumn>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<DataGrid, Empty.Data, Empty.State> presenter)
        {
            //FindPropertiesToInclude();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private void FindPropertiesToInclude()
        //{
        //    var metaType = MetadataCache.GetTypeMetadata(this.EntityName);

        //    foreach ( var column in DisplayColumns )
        //    {
        //        if ( column.Navigations.Length > 1 )
        //        {
        //            for ( int i = 0 ; i < column.Navigations.Length ; i++ )
        //            {
        //                IPropertyMetadata metaProperty;

        //                if ( metaType.TryGetPropertyByName(column.Navigations[i], out metaProperty) )
        //                {
        //                    if ( metaProperty.Relation != null && i < column.Navigations.Length - 1 && !metaProperty.Relation.RelatedPartyType.IsEntityPart )
        //                    {
        //                        IncludedProperties.Add(column.Expression);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        
        [DataMember]
        public List<GridColumn> DisplayColumns { get; set; }

        [DataMember]
        public List<GridColumn> DefaultDisplayColumns { get; set; }

        [DataMember]
        public bool UsePascalCase { get; set; }

        [DataMember]
        public DataGridMode Mode { get; set; }

        [DataMember]
        public bool? EnablePaging { get; set; }

        [DataMember]
        public bool EnableAutonomousQuery { get; set; }

        [DataMember]
        public bool IsInlineEditEnabled { get; set; }

        [DataMember]
        public bool IsInlineEditOpenFormEnabled { get; set; }

        [DataMember]
        public bool DisableFiltering { get; set; }

        [DataMember]
        public bool DisableSorting { get; set; }

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
        public string DetailPaneExpression { get; set; }

        [DataMember]
        public bool DetailPaneQueryServer { get; set; }

        [DataMember]
        public int[] PageSizeOptions { get; set; }

        [DataMember]
        public int? DefaultPageSize { get; set; }

        [DataMember]
        public DataGridSelectionMode SelectionMode { get; set; }

        [DataMember]
        public bool ExpandableTreeMode { get; set; }

        [DataMember]
        public string SubTreePropertyName { get; set; }

        [DataMember]
        public bool IgnoreTreeNodeServerIds { get; set; }

        [DataMember]
        public bool FlatStyle { get; set; }

        [DataMember]
        public string RowStylePropertyName { get; set; }

        [DataMember]
        public string RowIconPropertyName { get; set; }

        [DataMember]
        public bool HiddenMode { get; set; }

        [DataMember]
        public bool PassiveQueryMode { get; set; }

        //[DataMember]
        //public string DetailPaneStaticTemplateName { get; set; }

        //[DataMember]
        //public string DetailPaneTemplateNameProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<object> RequestPrepared { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns.Select(c => new LocaleEntryKey(c.Title, this, "DisplayColumns")));
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
                FieldSpecialName specialName,
                string title = null, 
                FieldSize size = FieldSize.Medium, 
                string format = null,
                GridColumnType? columnType = null)
                : this(
                    metaType, 
                    new[] { "$" + specialName.ToString().ToCamelCase() }, 
                    specialName, 
                    title, 
                    size, 
                    format, 
                    includeInTotal: false, 
                    columnType: columnType)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn(
                ITypeMetadata metaType, 
                LambdaExpression propertyNavigation, 
                string title = null, 
                FieldSize size = FieldSize.Medium, 
                string format = null,
                bool includeInTotal = false,
                GridColumnType? columnType = null)
                : this(metaType, ParsePropertyNavigation(propertyNavigation), FieldSpecialName.None, title, size, format, includeInTotal, columnType)
            {
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal GridColumn(ITypeMetadata metaType, FormField lookupField)
                : this(
                    metaType, 
                    navigations: new[] { lookupField.PropertyName, lookupField.LookupDisplayProperty },
                    columnType: GridColumnType.Hidden)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal GridColumn(
                ITypeMetadata metaType, 
                string[] navigations,
                FieldSpecialName specialName = FieldSpecialName.None,
                string title = null, 
                FieldSize size = FieldSize.Medium, 
                string format = null,
                bool includeInTotal = false,
                GridColumnType? columnType = null)
            {
                this.Navigations = navigations;
                this.Expression = string.Join(".", Navigations);
                this.SpecialName = specialName;

                this.Size = size;
                this.Format = format;
                this.IncludeInTotal = includeInTotal;

                ITypeMetadata destinationMetaType;
                IPropertyMetadata destinationMetaProperty;
                bool isManualJoinRequired;
                FindDeclaringMetaType(metaType, out destinationMetaType, out destinationMetaProperty, out isManualJoinRequired);

                this.Title = title ?? (destinationMetaProperty != null ? FormField.GetDefaultFieldLabel(destinationMetaProperty) : this.Navigations.Last());
                this.DeclaringTypeName = destinationMetaType.QualifiedName;
                this.MetaProperty = destinationMetaProperty;
                this.ColumnType = columnType.GetValueOrDefault(GetGridColumnType(MetaProperty));

                this.IsFilterSupported = (specialName == FieldSpecialName.None && !isManualJoinRequired);
                this.IsSortSupported = this.IsFilterSupported;
                this.IsEditSupported = (specialName == FieldSpecialName.None && MetaProperty.Kind == PropertyKind.Scalar && !isManualJoinRequired);

                if ( MetaProperty != null && MetaProperty.Relation != null )
                {
                    this.RelatedEntityName = MetaProperty.Relation.RelatedPartyType.QualifiedName;
                }

                if (format == null && MetaProperty != null)
                {
                    this.Format = GetColumnDefaultFormatString(MetaProperty);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn SetWidget(WidgetUidlNode widget)
            {
                this.ColumnType = GridColumnType.Widget;
                this.NestedWidget = widget;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn BindIconTo<TDataRow>(Expression<Func<TDataRow, object>> iconProperty)
            {
                this.IconPropertyName = iconProperty.GetPropertyInfo().Name;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn SortByDefault(bool ascending = true)
            {
                this.DefaultSort = true;
                this.DefaultSortAscending = ascending;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn DisplayCollectionItemsAs<TItem>(Expression<Func<TItem, object>> propertySelector)
            {
                this.ItemDisplayAsExpression = propertySelector.ToNormalizedNavigationString(skipSteps: 1);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn SearchUsingPropertyOf<TDataRow>(Expression<Func<TDataRow, object>> searchProperty)
            {
                this.SearchPropertyName = searchProperty.GetPropertyInfo().Name;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GridColumn BindClickableTo<TDataRow>(Expression<Func<TDataRow, bool>> clickableProperty)
            {
                this.ClickablePropertyName = clickableProperty.GetPropertyInfo().Name;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public FieldSpecialName SpecialName { get; set; }
            [DataMember]
            public GridColumnType ColumnType { get; set; }
            [DataMember]
            public string Title { get; set; }
            [DataMember]
            public FieldSize Size { get; set; }
            [DataMember]
            public string Format { get; set; }
            [DataMember]
            public WidgetAlignment Alignment { get; set; }
            [DataMember]
            public string Expression { get; set; }
            [DataMember]
            public string ItemDisplayAsExpression { get; set; }
            [DataMember]
            public string[] Navigations { get; set; }
            [DataMember]
            public string DeclaringTypeName { get; set; }
            [DataMember]
            public string SearchPropertyName { get; set; }
            [DataMember]
            public bool IncludeInTotal { get; set; }
            [DataMember]
            public bool IsSortSupported { get; set; }
            [DataMember]
            public bool IsFilterSupported { get; set; }
            [DataMember]
            public bool IsEditSupported { get; set; }
            [DataMember]
            public bool IsReadOnly { get; set; }
            [DataMember]
            public bool IsFilterDisabled { get; set; }
            [DataMember]
            public bool IsSortDisabled { get; set; }
            [DataMember]
            public string RelatedEntityName { get; set; }
            [DataMember, ManuallyAssigned]
            public UidlAuthorization Authorization { get; set; }
            [DataMember, ManuallyAssigned]
            public WidgetUidlNode NestedWidget { get; set; }
            [DataMember]
            public string IconPropertyName { get; set; }
            [DataMember]
            public bool DefaultSort { get; set; }
            [DataMember]
            public bool DefaultSortAscending { get; set; }
            [DataMember]
            public bool Clickable { get; set; }
            [DataMember]
            public string ClickablePropertyName { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal bool HasDataForLookupField(FormField field)
            {
                return (
                    Navigations.Length == 2 && 
                    Navigations[0] == field.PropertyName && 
                    Navigations[1] == field.LookupDisplayProperty);
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal protected IPropertyMetadata MetaProperty { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void FindDeclaringMetaType(
                ITypeMetadata metaType, 
                out ITypeMetadata destinationMetaType, 
                out IPropertyMetadata destinationMetaProperty, 
                out bool isManualJoinRequired)
            {
                destinationMetaType = metaType;
                isManualJoinRequired = false;

                if ( this.SpecialName != FieldSpecialName.None )
                {
                    destinationMetaProperty = null;
                    return;
                }

                for ( int i = 0; i < Navigations.Length - 1; i++ )
                {
                    var navigationMetaProperty = destinationMetaType.FindPropertyByNameIncludingDerivedTypes(Navigations[i]);

                    if ( i < Navigations.Length - 1 && navigationMetaProperty.Relation != null )
                    {
                        destinationMetaType = navigationMetaProperty.Relation.RelatedPartyType;

                        if ( navigationMetaProperty.ClrType != navigationMetaProperty.Relation.RelatedPartyType.ContractType )
                        {
                            isManualJoinRequired = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                destinationMetaProperty = destinationMetaType.FindPropertyByNameIncludingDerivedTypes(Navigations.Last());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            internal static string[] ParsePropertyNavigation(LambdaExpression propertyNavigation)
            {
                return propertyNavigation.ToNormalizedNavigationStringArray();
                //var expressionString = propertyNavigation.ToNormalizedNavigationString();
                //return expressionString.Split('.').Skip(1).ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static GridColumnType GetGridColumnType(IPropertyMetadata metaProperty)
            {
                if ( metaProperty == null )
                {
                    return GridColumnType.Text;
                }

                if ( metaProperty.Kind == PropertyKind.Relation || metaProperty.Relation != null )
                {
                    return (metaProperty.IsCollection ? GridColumnType.LookupMany : GridColumnType.Key);
                }

                if ( metaProperty.ClrType.IsAnyNumericType() )
                {
                    return GridColumnType.Number;
                }

                if ( metaProperty.ClrType.IsEnum )
                {
                    return GridColumnType.Enum;
                }

                if (metaProperty.ClrType == typeof(ChartData))
                {
                    return GridColumnType.Widget;
                }

                if ( metaProperty.SemanticType != null )
                {
                    var semantic = metaProperty.SemanticType.WellKnownSemantic;

                    switch ( semantic )
                    {
                        case WellKnownSemanticType.ImageUrl:
                            return GridColumnType.Image;
                        case WellKnownSemanticType.Url:
                            return GridColumnType.Link;
                    }
                }

                return GridColumnType.Text;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static string GetColumnDefaultFormatString(IPropertyMetadata property)
            {
                if (property.SemanticType != null)
                {
                    switch (property.SemanticType.WellKnownSemantic)
                    {
                        case WellKnownSemanticType.Date:
                            return "d";
                        case WellKnownSemanticType.Currency:
                            return "c";
                    }
                }

                if (property.ClrType == typeof(decimal))
                {
                    return "#,##0.00";
                }

                return null;
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
        where TDataRow : class
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
            string format = null,
            bool includeInTotal = false,
            GridColumnType? columnType = null,
            Action<GridColumn> setup = null)
        {
            var column = new GridColumn(MetaType, propertySelector, title, size, format, includeInTotal, columnType);

            if (setup != null)
            {
                setup(column);
            }

            this.DisplayColumns.Add(column);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> Column<TDerivedDataRow, T>(
            Expression<Func<TDerivedDataRow, T>> propertySelector, 
            string title = null, 
            FieldSize size = FieldSize.Medium,
            string format = null,
            bool includeInTotal = false,
            GridColumnType? columnType = null,
            Action<GridColumn> setup = null)
            where TDerivedDataRow : TDataRow
        {
            var derivedMetaType = MetadataCache.GetTypeMetadata(typeof(TDerivedDataRow));
            var column = new GridColumn(derivedMetaType, propertySelector, title, size, format, includeInTotal, columnType);

            if (setup != null)
            {
                setup(column);
            }

            this.DisplayColumns.Add(column);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> ColumnForType(
            string title = "Type", 
            FieldSize size = FieldSize.Medium,
            Action<GridColumn> setup = null)
        {
            var column = new GridColumn(MetaType, FieldSpecialName.Type, title, size, columnType: GridColumnType.Type);
            
            if (setup != null)
            {
                setup(column);
            }

            column.IsFilterSupported = true;
            column.IsSortSupported = false;

            this.DisplayColumns.Add(column);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> ColumnWithNavigationTo<TDestinationEntity>(
            Expression<Func<TDataRow, object>> sourcePropertySelector,
            Expression<Func<TDestinationEntity, object>> destinationPropertySelector,
            string title = null,
            FieldSize size = FieldSize.Medium,
            string format = null,
            GridColumnType? columnType = null,
            Action<GridColumn> setup = null)
        {
            var navigations =
                GridColumn.ParsePropertyNavigation(sourcePropertySelector)
                .Concat(GridColumn.ParsePropertyNavigation(destinationPropertySelector))
                .ToArray();

            var column = new GridColumn(MetaType, navigations, FieldSpecialName.None, title, size, format, columnType: columnType);

            if (setup != null)
            {
                setup(column);
            }

            this.DisplayColumns.Add(column);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> ColumnWithCollectionNavigation<TDestinationEntity>(
            Expression<Func<TDataRow, ICollection<TDestinationEntity>>> sourcePropertySelector,
            Expression<Func<TDestinationEntity, object>> destinationPropertySelector,
            string title = null,
            FieldSize size = FieldSize.Medium,
            string format = null,
            GridColumnType? columnType = null,
            Action<GridColumn> setup = null)
        {
            var navigations =
                GridColumn.ParsePropertyNavigation(sourcePropertySelector)
                .Concat(GridColumn.ParsePropertyNavigation(destinationPropertySelector))
                .ToArray();

            var column = new GridColumn(MetaType, navigations, FieldSpecialName.None, title, size, format, columnType: columnType);

            if (setup != null)
            {
                setup(column);
            }

            this.DisplayColumns.Add(column);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> EnableExpandableTree(Expression<Func<TDataRow, System.Collections.IEnumerable>> subTreeProperty)
        {
            var subTreePropertyInfo = subTreeProperty.GetPropertyInfo();
            var subTreeMetaProperty = this.MetaType.GetPropertyByDeclaration(subTreePropertyInfo);

            this.ExpandableTreeMode = true;
            this.SubTreePropertyName = subTreePropertyInfo.Name;

            this.IgnoreTreeNodeServerIds = (
                this.MetaType.IsEntityPart || 
                (subTreeMetaProperty.Relation != null && subTreeMetaProperty.Relation.RelatedPartyType.IsEntityPart));

            return this;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> BindRowIconTo(Expression<Func<TDataRow, object>> iconProperty)
        {
            this.RowIconPropertyName = iconProperty.GetPropertyInfo().Name;
            return this;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> BindRowStyleTo(Expression<Func<TDataRow, object>> styleProperty)
        {
            this.RowStylePropertyName = styleProperty.GetPropertyInfo().Name;
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

        public DataGrid<TDataRow> UseDetailPane(
            WidgetUidlNode widget, 
            bool expanded = false, 
            Expression<Func<TDataRow, object>> expression = null,
            bool queryServer = false)
        {
            this.EnableDetailPane = true;
            this.DetailPaneWidget = widget;
            //this.DetailPaneStaticTemplateName = null;
            //this.DetailPaneTemplateNameProperty = null;
            this.DetailPaneExpanded = expanded;
            this.DetailPaneExpression = (expression != null ? expression.ToNormalizedNavigationString(skipSteps: 1) : null);
            this.DetailPaneQueryServer = queryServer;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> UseInlineEditor()
        {
            this.IsInlineEditEnabled = true;
            this.InlineEditor = new Form<TDataRow>("InlineEditor", this);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public DataGrid<TDataRow> UseBackendTransactionScript<TTxContext>()
        //{
        //    this.IsInlineEditEnabled = true;
        //    this.InlineEditor = new Form<TDataRow>("InlineEditor", this);

        //    return this;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<TDataRow> PageSize(int defaultSize, int[] pageSizeOptions)
        {
            this.EnablePaging = true;
            this.DefaultPageSize = defaultSize;
            this.PageSizeOptions = pageSizeOptions;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().ConcatIf(this.InlineEditor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public Form<TDataRow> InlineEditor { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<IEnumerable<TDataRow>> DataReceived { get; set; }
        public UidlNotification<TDataRow> DataRowReceived { get; set; }
        public UidlNotification<ApplicationEntityService.QueryResults> QueryCompleted { get; set; }
        public UidlNotification<IPromiseFailureInfo> QueryFailed { get; set; }
        public UidlNotification<ICellClickInfo> CellClicked { get; set; }
        public UidlNotification<TDataRow> SavingInlineRowEdits { get; set; }

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

        protected override void OnBuildManuallyInstantiatedNodes(UidlBuilder builder)
        {
            if (InlineEditor != null)
            {
                builder.BuildManuallyInstantiatedNodes(InlineEditor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void PostDescribePresenter(PresenterBuilder<DataGrid, Empty.Data, Empty.State> presenter)
        {
            if (IsInlineEditEnabled && InlineEditor != null)
            {
                InlineEditor.FilterFields(f => (f.Modifiers & (FormFieldModifiers.Tab | FormFieldModifiers.Section)) == 0);

                var editableFields = InlineEditor.Fields
                    .Where(f => !f.Modifiers.HasFlag(FormFieldModifiers.ReadOnly))
                    .ToDictionary(f => f.PropertyName);

                foreach (var column in this.DisplayColumns)
                {
                    column.IsEditSupported = (editableFields.ContainsKey(column.Expression));
                }

                foreach (var column in this.DefaultDisplayColumns)
                {
                    column.IsEditSupported = (editableFields.ContainsKey(column.Expression));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void EnsureDataForLookupField(FormField field)
        {
            if (!DisplayColumns.Any(c => c.HasDataForLookupField(field)))
            {
                DisplayColumns.Add(new GridColumn(MetaType, field));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldDisplayPropertyByDefault(IPropertyMetadata metaProperty)
        {
            return (
                (metaProperty.Role == PropertyRole.None && metaProperty.Kind == PropertyKind.Scalar) ||
                (metaProperty == metaProperty.DeclaringContract.DisplayNameProperty) ||
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

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ICellClickInfo
        {
            TDataRow Data { get; set; }
            string ColumnExpression { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum DataGridMode
    {
        Standalone = 10,
        Inline = 20,
        LookupOne = 30,
        LookupMany = 40
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum DataGridSelectionMode
    {
        Single = 0,
        Multiple = 10,
        Disabled = 20
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum GridColumnType
    {
        Text = 10,
        Number = 20,
        Enum = 30,
        Image = 40,
        Link = 50,
        Key = 60,
        Widget = 70,
        Type = 80,
        Lookup = 90,
        LookupMany = 100,
        Hidden = 1000
    }
}

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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<DataGrid, Empty.Data, Empty.State> presenter)
        {
            if ( RowTemplate == null )
            {
                RowTemplate = DefaultRowTemplate;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        
        //TODO: allow uniform query for both entities and transacrion script results
        //[DataMember]
        //public string DataQuery { get; set; }
        
        [DataMember]
        public List<GridColumn> DisplayColumns { get; set; }
        
        [DataMember]
        public List<GridColumn> DefaultDisplayColumns { get; set; }
        
        [DataMember, ManuallyAssigned]
        public WidgetUidlNode RowTemplate { get; set; }

        [DataMember]
        public DataGridDefaultRow DefaultRowTemplate { get; set; }

        [DataMember]
        public bool UsePascalCase { get; set; }

        [DataMember]
        public DataGridMode Mode { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns.Select(c => c.Title));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetBase<DataGrid,Data,State>

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.BuildManuallyInstantiatedNodes(RowTemplate);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
        public class GridColumn
        {
            public GridColumn(ITypeMetadata metaType, LambdaExpression propertyNavigation, string title = null, FieldSize size = FieldSize.Small, string format = null)
            {
                var expressionString = propertyNavigation.ToNormalizedNavigationString(convertToCamelCase: false);

                this.Navigations = expressionString.Split('.').Skip(1).ToArray();
                this.Expression = string.Join(".", Navigations);

                this.Title = title ?? this.Navigations.Last();
                this.Size = size;
                this.Format = format;
                this.DeclaringTypeName = FindDeclaringMetaType(metaType).QualifiedName;
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private ITypeMetadata FindDeclaringMetaType(ITypeMetadata metaType)
            {
                var destinationMetaType = metaType;

                for ( int i = 0; i < Navigations.Length - 1; i++ )
                {
                    var destinationMetaProperty = destinationMetaType.GetPropertyByName(Navigations[i]);

                    if ( (destinationMetaProperty.Kind == PropertyKind.Part || destinationMetaProperty.Kind == PropertyKind.Relation) &&
                        destinationMetaProperty.Relation != null )
                    {
                        destinationMetaType = destinationMetaProperty.Relation.RelatedPartyType;
                    }
                    else
                    {
                        break;
                    }
                }

                return destinationMetaType;
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

        public DataGrid<TDataRow> Column<T>(Expression<Func<TDataRow, T>> propertySelector, string title = null, FieldSize size = FieldSize.Small)
        {
            this.DisplayColumns.Add(new GridColumn(MetaType, propertySelector, title, size));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<IEnumerable<TDataRow>> DataReceived { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        internal ITypeMetadata MetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.RegisterMetaType(typeof(TDataRow));
            base.EntityName = MetadataCache.GetTypeMetadata(typeof(TDataRow)).QualifiedName;

            base.DefaultDisplayColumns = MetaType.Properties
                .Where(ShouldDisplayPropertyByDefault)
                .Select(p => new GridColumn(MetaType, MetaType.MakePropertyExpression(p)))
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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum DataGridMode
    {
        Standalone,
        Inline,
        LookupOne,
        LookupMany,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI;
using NWheels.UI.Toolbox;

namespace NWheels.Processing.Documents
{
    public class DocumentDesign
    {
        public DocumentDesign(
            string idName, 
            Element contents,
            DocumentDesingOptions options = null)
        {
            this.IdName = idName;
            this.Contents = contents;
            this.Options = options ?? _s_defaultOptions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string IdName { get; private set; }
        public Element Contents { get; private set; }
        public DocumentDesingOptions Options { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly DocumentDesingOptions _s_defaultOptions = new DocumentDesingOptions();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityTableBuilder<TEntity> EntityTable<TEntity>(ITypeMetadataCache metadataCache)
        {
            return new EntityTableBuilder<TEntity>(metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DocumentDesingOptions DefaultOptions
        {
            get { return _s_defaultOptions; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class Element
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ContainerElement : Element, IEnumerable<Element>
        {
            protected ContainerElement(params Element[] contents)
            {
                this.Contents = new List<Element>(contents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IList<Element> Contents { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
            {
                return Contents.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return Contents.GetEnumerator();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TableElement : Element
        {
            public TableElement(params Column[] columns)
            {
                this.Columns = new List<Column>(columns);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string BoundEntityName { get; set; }
            public IList<Column> Columns { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class Column
            {
                public Column(string title, double? width, Binding binding)
                {
                    Title = title;
                    Width = width;
                    Binding = binding;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public string Title { get; private set; }
                public double? Width { get; private set; }
                public Binding Binding { get; private set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Binding
        {
            public Binding(string expression, string format, string fallback = null, bool isKey = false)
            {
                SpecialName = FieldSpecialName.None;
                Expression = expression;
                Format = format;
                Fallback = fallback;
                IsKey = isKey;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Binding(FieldSpecialName specialName, string format, string fallback = null, bool isKey = false)
            {
                SpecialName = specialName;
                Expression = null;
                Format = format;
                Fallback = fallback;
                IsKey = isKey;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int TryFindCursorColumnIndex(ApplicationEntityService.EntityCursor cursor)
            {
                for (int cursorColumnIndex = 0; cursorColumnIndex < cursor.ColumnCount; cursorColumnIndex++)
                {
                    var cursorColumnExpression = string.Join(".", cursor.Columns[cursorColumnIndex].PropertyPath);
                    
                    if (cursorColumnExpression == this.Expression)
                    {
                        return cursorColumnIndex;
                    }
                }

                return -1;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object ReadValueFromCursor(ApplicationEntityService.EntityCursorRow cursorRow, int columnIndex, bool applyFormat)
            {
                if (SpecialName == FieldSpecialName.Id)
                {
                    return cursorRow.Metadata.EntityMetaType.EntityIdProperty.ReadValue(cursorRow.Record);
                }

                if (SpecialName == FieldSpecialName.Type)
                {
                    return cursorRow.Metadata.EntityMetaType.QualifiedName;
                }

                //var declaringContract = cursorRow.Metadata.Columns[columnIndex].MetaProperty.ContractPropertyInfo.DeclaringType;

                //if (!declaringContract.IsAssignableFrom(cursorRow.Record.ContractType))
                //{
                //    return Fallback;
                //}
                
                if (columnIndex < 0)
                {
                    return "#BINDERR";
                }

                var value = cursorRow[columnIndex];

                if ((value == null || value.Equals(string.Empty)) && Fallback != null)
                {
                    return Fallback;
                }

                if (applyFormat && value != null)
                {
                    return string.Format("{0:" + Format + "}", value);
                }

                return value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FieldSpecialName SpecialName { get; private set; }
            public string Expression { get; private set; }
            public string Format { get; private set; }
            public string Fallback { get; private set; }
            public bool IsKey { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityTableBuilder<TEntity> 
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly ITypeMetadata _metaType;
            private TableElement _element;
            private string _navigationPrefix;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder(ITypeMetadataCache metadataCache)
            {
                _metadataCache = metadataCache;
                _metaType = metadataCache.GetTypeMetadata(typeof(TEntity));
                _element = new TableElement();
                _element.BoundEntityName = _metaType.QualifiedName;
                _navigationPrefix = "";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity2> NavigateTo<TEntity2>(Expression<Func<TEntity, TEntity2>> navigation)
            {
                var nextNavigations = navigation.ToNormalizedNavigationStringArray();
                var nextNavigationPrefix = string.Join(".", nextNavigations) + ".";
                var newBuilder = new EntityTableBuilder<TEntity2>(_metadataCache);

                newBuilder._element = _element;
                newBuilder._navigationPrefix = _navigationPrefix + nextNavigationPrefix;
                
                return newBuilder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity2> NavigateToCollection<TEntity2>(Expression<Func<TEntity, ICollection<TEntity2>>> navigation)
            {
                var nextNavigations = navigation.ToNormalizedNavigationStringArray();
                var nextNavigationPrefix = string.Join(".", nextNavigations) + ".";
                var newBuilder = new EntityTableBuilder<TEntity2>(_metadataCache);

                newBuilder._element = _element;
                newBuilder._navigationPrefix = _navigationPrefix + nextNavigationPrefix;

                return newBuilder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> Column<T>(
                Expression<Func<TEntity, T>> property, 
                string title = null, 
                double? width = null, 
                string format = null, 
                string fallback = null, 
                bool isKey = false)
            {
                return InternalColumn(property, title, width, format, fallback, isKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> ColumnWithNavigationTo<TTargetEntity>(
                Expression<Func<TEntity, object>> pathToEntityId,
                Expression<Func<TTargetEntity, object>> pathWithinTargetEntity,
                string title = null,
                double? width = null,
                string format = null,
                string fallback = null,
                bool isKey = false)
            {
                var pathWithinTargetEntityArray = pathWithinTargetEntity.ToNormalizedNavigationStringArray();

                var bindingExpression = 
                    _navigationPrefix +
                    string.Join(".", pathToEntityId.ToNormalizedNavigationStringArray()) + 
                    "." +
                    string.Join(".", pathWithinTargetEntityArray);

                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace(pathWithinTargetEntityArray.Last()),
                    width,
                    new Binding(bindingExpression, format, fallback, isKey));

                _element.Columns.Add(column);

                return this;
            }

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public EntityTableBuilder<TEntity> InheritorColumnWithNavigationTo<TInheritor, TTargetEntity>(
            //    Expression<Func<TEntity, object>> pathToEntityId,
            //    Expression<Func<TTargetEntity, object>> pathWithinTargetEntity,
            //    string title = null,
            //    double? width = null,
            //    string format = null,
            //    string fallback = null,
            //    bool isKey = false)
            //{
            //    var pathWithinTargetEntityArray = pathWithinTargetEntity.ToNormalizedNavigationStringArray();

            //    var bindingExpression =
            //        _navigationPrefix +
            //        string.Join(".", pathToEntityId.ToNormalizedNavigationStringArray()) +
            //        "." +
            //        string.Join(".", pathWithinTargetEntityArray);

            //    var column = new TableElement.Column(
            //        title.OrDefaultIfNullOrWhitespace(pathWithinTargetEntityArray.Last()),
            //        width,
            //        new Binding(bindingExpression, format, fallback, isKey));

            //    _element.Columns.Add(column);

            //    return this;
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> InheritorColumn<TInheritorEntity>(
                Expression<Func<TInheritorEntity, object>> property,
                string title = null,
                double? width = null,
                string format = null,
                string fallback = null,
                bool isKey = false)
                where TInheritorEntity : TEntity
            {
                return InternalColumn(property, title, width, format, fallback, isKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> MemberInheritorColumn<TAncestor, TInheritor>(
                Expression<Func<TEntity, TAncestor>> pathToAncestor,
                Expression<Func<TInheritor, object>> pathWithinInheritor,
                string title = null,
                double? width = null,
                string format = null,
                string fallback = null,
                bool isKey = false)
                where TInheritor : TAncestor
            {
                var pathWithinAncestorArray = pathWithinInheritor.ToNormalizedNavigationStringArray();

                var bindingExpression = 
                    _navigationPrefix +
                    string.Join(".", pathToAncestor.ToNormalizedNavigationStringArray()) + 
                    "." +
                    string.Join(".", pathWithinAncestorArray);

                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace(pathWithinAncestorArray.Last()),
                    width,
                    new Binding(bindingExpression, format, fallback, isKey));

                _element.Columns.Add(column);

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> TypeColumn(
                string title = null,
                double? width = null,
                string format = null,
                string fallback = null,
                bool isKey = false)
            {
                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace("Type"),
                    width,
                    new Binding(FieldSpecialName.Type, format, fallback, isKey));

                _element.Columns.Add(column);

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> IdColumn(
                string title = null,
                double? width = null,
                string format = null,
                string fallback = null)
            {
                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace("Id"),
                    width,
                    new Binding(FieldSpecialName.Id, format, fallback, isKey: true));

                _element.Columns.Add(column);

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private EntityTableBuilder<TEntity> InternalColumn(
                LambdaExpression propertyExpression,
                string title = null,
                double? width = null,
                string format = null,
                string fallback = null,
                bool isKey = false)
            {
                var normalizedNavigationArray = propertyExpression.ToNormalizedNavigationStringArray();
                var bindingExpression = _navigationPrefix + string.Join(".", normalizedNavigationArray);

                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace(normalizedNavigationArray.Last()),
                    width,
                    new Binding(bindingExpression, format, fallback, isKey));

                _element.Columns.Add(column);

                return this;
            }

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //private IPropertyMetadata GetPropertyMetadata(PropertyInfo propertyInfo)
            //{
            //    IPropertyMetadata metaProperty;
            
            //    if (propertyInfo.DeclaringType.IsAssignableFrom(_metaType.ContractType))
            //    {
            //        metaProperty = _metaType.GetPropertyByDeclaration(propertyInfo);
            //    }
            //    else
            //    {
            //        var inheritorMetaType = _metaType.DerivedTypes.First(t => propertyInfo.DeclaringType.IsAssignableFrom(t.ContractType));
            //        metaProperty = inheritorMetaType.GetPropertyByDeclaration(propertyInfo);
            //    }
                
            //    return metaProperty;
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static implicit operator TableElement(EntityTableBuilder<TEntity> builder)
            {
                return builder._element;
            }
        }
    }
}

using System.Reflection;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;

namespace NWheels.UI.Model.Impl.Parsers
{
    public class ComponentParsers
    {
        public TextContentMetadata TextContent(PreprocessedProperty prop)
        {
            return new TextContentMetadata(new MetadataObjectHeader(prop)) {
                Text = prop.ConstructorArguments[0].ClrValue as string,
                MapStateToValue = prop.ConstructorArguments[0].ValueExpression
            };
        }

        public FormMetadata Form(PreprocessedProperty prop)
        {
            return new FormMetadata(new MetadataObjectHeader(prop)) {
                MapStateToValue = prop.ConstructorArguments[0].ValueExpression
            };
        }

        public DataGridMetadata DataGrid(PreprocessedProperty prop)
        {
            return new DataGridMetadata(new MetadataObjectHeader(prop)) {
                MapStateToValue = prop.ConstructorArguments[0].ValueExpression
            };
        }

        public GridLayoutMetadata GridLayout(PreprocessedProperty prop)
        {
            return new GridLayoutMetadata(new MetadataObjectHeader(prop)) {
               // MapStateToValue = prop.ConstructorArguments[0].ValueExpression
            };
        }

        public SeatingPlanMetadata SeatingPlan(PreprocessedProperty prop)
        {
            return new SeatingPlanMetadata(new MetadataObjectHeader(prop)) {
                MapStateToValue = prop.ConstructorArguments[0].ValueExpression
            };
        }
    }
}

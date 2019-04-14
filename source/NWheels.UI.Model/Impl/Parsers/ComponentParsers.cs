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
            return new TextContentMetadata(MetadataObjectHeader.NoSourceType()) {
                Text = prop.ConstructorArguments[0].ClrValue as string 
            };
        }

        public FormMetadata Form(PreprocessedProperty prop)
        {
            return new FormMetadata(MetadataObjectHeader.NoSourceType()) {
            };
        }

        public DataGridMetadata DataGrid(PreprocessedProperty prop)
        {
            return new DataGridMetadata(MetadataObjectHeader.NoSourceType()) {
            };
        }

        public GridLayoutMetadata GridLayout(PreprocessedProperty prop)
        {
            return new GridLayoutMetadata(MetadataObjectHeader.NoSourceType()) {
            };
        }

        public SeatingPlanMetadata SeatingPlan(PreprocessedProperty prop)
        {
            return new SeatingPlanMetadata(MetadataObjectHeader.NoSourceType()) {
            };
        }
    }
}

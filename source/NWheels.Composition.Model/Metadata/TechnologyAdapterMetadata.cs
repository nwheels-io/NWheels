using System;
using System.Collections.Generic;
using System.Linq;
using MetaPrograms.Expressions;

namespace NWheels.Composition.Model.Metadata
{
    public class TechnologyAdapterMetadata
    {
        public TechnologyAdapterMetadata(Type adapterType, IReadOnlyDictionary<string, object> parameters)
        {
            AdapterType = adapterType;
            Parameters = parameters;
        }

        public Type AdapterType { get; }
        
        // TODO: support configuration classes, not only scalar key-value pairs
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public static TechnologyAdapterMetadata FromSource(PreprocessedTechnologyAdapter source)
        {
            var parameters = source.AdapterArguments.ToDictionary(
                arg => arg.Name,
                arg => (arg.ValueExpression as ConstantExpression)?.Value);
            
            return new TechnologyAdapterMetadata(source.ClrType, parameters);
        }
    }
}

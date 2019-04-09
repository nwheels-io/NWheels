using System;
using System.Collections.Generic;
using System.Linq;
using MetaPrograms.CSharp;
using MetaPrograms.Expressions;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class TechnologyAdapterMetadata
    {
        public TechnologyAdapterMetadata(Type adapterType, IReadOnlyDictionary<string, object> parameters)
        {
            AdapterType = adapterType;
            Parameters = parameters;
        }

        public Type AdapterType { get; }
        
        // TODO: support generic arguments, e.g. for a call like AsSomeTechnology<T>()
        // TODO: support configuration classes, not only scalar key-value pairs
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public static TechnologyAdapterMetadata FromSource(PreprocessedTechnologyAdapter source)
        {
            var parameters = source.AdapterArguments.ToDictionary(
                arg => arg.Name,
                arg => arg.ValueExpression?.GetConstantValueOrNull());
            
            return new TechnologyAdapterMetadata(source.ClrType, parameters);
        }
    }
}

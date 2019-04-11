using System.Collections.Generic;
using System.Linq;
using MetaPrograms;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class MetadataObjectHeader
    {
        public MetadataObjectHeader(PreprocessedType sourceType)
            : this(
                sourceType, 
                name: sourceType?.ConcreteType.Name,
                namespaceName: sourceType?.ConcreteType.Namespace,
                qualifiedName: sourceType?.ConcreteType.FullName)
        {
            
        }

        public MetadataObjectHeader(
            PreprocessedType sourceType, 
            IdentifierName name, 
            string namespaceName, 
            string qualifiedName)
        {
            SourceType = sourceType;
            Name = name;
            Namespace = namespaceName;
            QualifiedName = qualifiedName;
            TechnologyAdapters = sourceType?.ReferencedBy
                .Where(refProp => refProp.TechnologyAdapter != null)
                .Select(refProp => TechnologyAdapterMetadata.FromSource(refProp.TechnologyAdapter))
                .ToArray()
                ?? new TechnologyAdapterMetadata[0];
        }

        public string Namespace { get; }
        public IdentifierName Name { get; }
        public string QualifiedName { get; }
        public PreprocessedType SourceType { get; }
        public IReadOnlyList<TechnologyAdapterMetadata> TechnologyAdapters { get; }
        public DeploymentScript DeploymentScript { get; } = new DeploymentScript();
        public ExtensionCollection Extensions { get; } = new ExtensionCollection();

        public static MetadataObjectHeader NoSourceType()
        {
            return new MetadataObjectHeader(null);
        }

        public static implicit operator MetadataObjectHeader(PreprocessedType sourceType)
        {
            return new MetadataObjectHeader(sourceType);
        }
    }
}

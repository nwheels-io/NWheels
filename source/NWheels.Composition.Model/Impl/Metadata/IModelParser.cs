using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface IModelParser
    {
        MetadataObject CreateMetadataObject(IModelPreParserContext context);
        void Parse(IModelParserContext context);
    }

    public interface IModelParserWithInit
    {
        void Initialize(IModelPreParserContext context);
    }
    
    public interface IModelPreParserContext
    {
        IReadOnlyPreprocessorOutput Preprocessor { get; }
        PreprocessedType Input { get; }
        ImperativeCodeModel Code { get; }
        RoslynCodeModelReader CodeReader { get; }
    }

    public interface IModelParserContext : IModelPreParserContext
    {
        void AddMoreOutputs(params MetadataObject[] outputs);
        MetadataObject TryGetMetadata(TypeMember concreteType);
        MetadataObject GetMetadata(TypeMember concreteType);
        MetadataObject Output { get; } 
    }
}

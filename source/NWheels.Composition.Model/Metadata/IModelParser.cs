using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;

namespace NWheels.Composition.Model.Metadata
{
    public interface IModelParserContext
    {
        IReadOnlyPreprocessorOutput Preprocessor { get; }
        PreprocessedTypeMember Input { get; }
        IList<MetadataObject> Output { get; }
        ImperativeCodeModel Code { get; }
        RoslynCodeModelReader CodeReader { get; }
    }
    
    public interface IModelParser
    {
        void Parse(IModelParserContext context);
    }
}

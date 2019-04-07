using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface IModelParser
    {
        void Parse(IModelParserContext context);
    }
    
    public interface IModelParserContext
    {
        IReadOnlyPreprocessorOutput Preprocessor { get; }
        PreprocessedType Input { get; }
        IList<MetadataObject> Output { get; }
        ImperativeCodeModel Code { get; }
        RoslynCodeModelReader CodeReader { get; }
    }
}

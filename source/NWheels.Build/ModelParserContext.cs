using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Members;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class ModelParserContext : IModelParserContext
    {
        private readonly ImperativeCodeModel _code;
        private readonly RoslynCodeModelReader _codeReader;
        private readonly IReadOnlyPreprocessorOutput _preprocessor;
        private readonly List<MetadataObject> _allOutputs;
        private readonly PreprocessedType _input;
        private readonly MetadataObject _output;

        public ModelParserContext(
            ImperativeCodeModel code,
            RoslynCodeModelReader codeReader,
            IReadOnlyPreprocessorOutput preprocessor,
            List<MetadataObject> allOutputs,
            PreprocessedType input = null,
            MetadataObject output = null)
        {
            _preprocessor = preprocessor;
            _code = code;
            _codeReader = codeReader;
            _allOutputs = allOutputs;
            _input = input;
            _output = output;
        }

        ImperativeCodeModel IModelPreParserContext.Code => _code;

        RoslynCodeModelReader IModelPreParserContext.CodeReader => _codeReader;

        IReadOnlyPreprocessorOutput IModelPreParserContext.Preprocessor => _preprocessor;

        MetadataObject IModelParserContext.GetMetadata(TypeMember concreteType)
        {
            return _preprocessor.GetByConcreteType(concreteType).ParsedMetadata;
        }

        MetadataObject IModelParserContext.TryGetMetadata(TypeMember concreteType)
        {
            return _preprocessor.TryGetByConcreteType(concreteType)?.ParsedMetadata;
        }

        void IModelParserContext.AddMoreOutputs(params MetadataObject[] outputs)
        {
            _allOutputs.AddRange(outputs);
        }

        PreprocessedType IModelPreParserContext.Input => _input;

        MetadataObject IModelParserContext.Output => _output;

        public ModelParserContext WithInput(PreprocessedType input)
        {
            return new ModelParserContext(
                _code, _codeReader, _preprocessor, _allOutputs, input, input.ParsedMetadata);            
        }

        public PreprocessedType Input => _input;
        public IMetadataObject Output => _output;
        public IList<MetadataObject> AllOutputs => _allOutputs;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class Preprocessor
    {
        private readonly ImperativeCodeModel _code;
        private readonly RoslynCodeModelReader _reader;
        private readonly TypeMember _modelParserAttributeType;

        public Preprocessor(ImperativeCodeModel code, RoslynCodeModelReader reader)
        {
            _code = code;
            _reader = reader;
            _modelParserAttributeType = code.GetClrTypeMember<ModelParserAttribute>();
        }

        public IReadOnlyPreprocessorOutput Run()
        {
            var output = new PreprocessorOutput();

            foreach (var type in _code.TopLevelMembers.OfType<TypeMember>())
            {
                if (IsParseableType(type, out var parserInfo))
                {
                    output.AddType(new PreprocessedType(type, parserInfo));
                }
            }

            return output;
        }

        private bool IsParseableType(TypeMember type, out ModelParserInfo parserInfo)
        {
            while (type != null)
            {
                if (TryGetParserType(type, out var parserType))
                {
                    parserInfo = new ModelParserInfo(type, parserType);
                    return true;
                }
            }

            parserInfo = null;
            return false;
        }

        private bool TryGetParserType(TypeMember type, out TypeMember parserType)
        {
            var parserTypeExpression = type
                .Attributes.FirstOrDefault(attr => attr.AttributeType == _modelParserAttributeType)
                ?.PropertyValues.FirstOrDefault(pv => pv.Name == nameof(ModelParserAttribute.Parser));

            parserType = (parserTypeExpression?.Value as TypeReferenceExpression)?.TypeOperand;
            return (parserType != null);
        }
    }
}

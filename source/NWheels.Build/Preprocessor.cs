using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class Preprocessor
    {
        private readonly ImperativeCodeModel _code;
        private readonly RoslynCodeModelReader _reader;
        private readonly TypeMember _includeAttributeType;
        private readonly TypeMember _modelParserAttributeType;

        public Preprocessor(ImperativeCodeModel code, RoslynCodeModelReader reader)
        {
            _code = code;
            _reader = reader;
            _includeAttributeType = code.GetClrTypeMember<IncludeAttribute>();
            _modelParserAttributeType = code.GetClrTypeMember<ModelParserAttribute>();
        }

        public IReadOnlyPreprocessorOutput Run()
        {
            Console.WriteLine("--- preprocessor: starting ---");

            var output = new PreprocessorOutput();

            try
            {
                DiscoverParseableTypes();
                CompleteParsing();
            }
            catch
            {
                Console.WriteLine("--- preprocessor: FAILURE ---");
                throw;
            }

            Console.WriteLine("--- preprocessor: success ---");

            return output;

            void DiscoverParseableTypes()
            {
                foreach (var type in _code.TopLevelMembers.OfType<TypeMember>())
                {
                    if (IsParseableType(type, out var parserInfo))
                    {
                        output.AddType(new PreprocessedType(type, parserInfo));
                    }
                }
            }

            void CompleteParsing()
            {
                foreach (var type in output.GetAll())
                {
                    Console.WriteLine($"Preprocessing type: {type.ConcreteType.FullName}");
                    CompleteParsingOfType(type);
                }
            }
            
            void CompleteParsingOfType(PreprocessedType type)
            {
                var parseableProperties = type.ConcreteType.Members
                    .OfType<PropertyMember>()
                    .Where(IsParseableProperty);

                foreach (var property in parseableProperties)
                {
                    Console.WriteLine($" - property {property.Name}");
                }
            }

            bool IsParseableType(TypeMember type, out ModelParserInfo parserInfo)
            {
                bool canBeParseable = (type.Status == MemberStatus.Parsed && type.Modifier != MemberModifier.Static);

                if (canBeParseable)
                {
                    while (type != null)
                    {
                        if (TryGetParserType(type, out var parserType))
                        {
                            parserInfo = new ModelParserInfo(type, parserType);
                            return true;
                        }

                        type = type.BaseType;
                    }
                }

                parserInfo = null;
                return false;
            }

            bool IsParseableProperty(PropertyMember property)
            {
                return (
                    property.Modifier != MemberModifier.Abstract && 
                    property.Modifier != MemberModifier.Static && 
                    property.Attributes.Any(attr => attr.AttributeType == _includeAttributeType));
            }

            bool TryGetParserType(TypeMember type, out TypeMember parserType)
            {
                var parserTypeExpression = type
                    .Attributes
                    .FirstOrDefault(attr => attr.AttributeType == _modelParserAttributeType)
                    ?.ConstructorArguments
                    .FirstOrDefault();

                parserType = ((parserTypeExpression as ConstantExpression)?.Value is INamedTypeSymbol parserTypeSymbol 
                    ? _code.TryGet<TypeMember>(binding: parserTypeSymbol) 
                    : null);
                
                return (parserType != null);
            }
            
        }

        //            bool TryParseProperty(PropertyMember property, out PreprocessedProperty property)
//            {
//                while (type != null)
//                {
//                    if (TryGetParserType(type, out var parserType))
//                    {
//                        parserInfo = new ModelParserInfo(type, parserType);
//                        return true;
//                    }
//
//                    type = type.BaseType;
//                }
//
//                parserInfo = null;
//                return false;
//            }


    }
}

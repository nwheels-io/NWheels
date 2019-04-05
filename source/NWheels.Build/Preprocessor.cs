using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using MetaPrograms.Statements;
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
            Console.WriteLine("--- preprocessor ---");

            var output = new PreprocessorOutput();

            DiscoverParseableTypes();
            CompletePreprocessing();
            PopulateReferencedBy();

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

            void CompletePreprocessing()
            {
                foreach (var outType in output.GetAll())
                {
                    Console.WriteLine($"Type: {outType.ConcreteType.FullName}");
                    CompletePreprocessingOfType(outType);
                }
            }

            void PopulateReferencedBy()
            {
                foreach (var type in output.GetAll())
                {
                    foreach (var prop in type.GetAllProperties())
                    {
                        var referencedType = output.TryGetByConcreteType(prop.Type);
                        referencedType?.ReferencedBy.Add(prop);
                    }
                }
            }
            
            void CompletePreprocessingOfType(PreprocessedType outType)
            {
                //TODO: create correct groups according to ICanInclude<T> interface
                outType.PropertyGroups.Add(new PreprocessedPropertyGroup(discriminator: null));
                
                var parseableProperties = outType.ConcreteType.Members
                    .OfType<PropertyMember>()
                    .Where(IsParseableProperty);

                foreach (var inProp in parseableProperties)
                {
                    Console.WriteLine($"Property: {inProp.DeclaringType.Name}::{inProp.Name}");
                    PreprocessProperty(outType, inProp);
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
            
            bool IsParseableProperty(PropertyMember property)
            {
                return (
                    property.Modifier != MemberModifier.Abstract && 
                    property.Modifier != MemberModifier.Static && 
                    property.Attributes.Any(attr => attr.AttributeType == _includeAttributeType));
            }

            void PreprocessProperty(PreprocessedType outType, PropertyMember inProp)
            {
                var propPreprocessor = new PropertyPreprocessor(_code, _reader, output);
                propPreprocessor.AddProperty(outType, inProp);
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

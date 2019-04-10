using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Composition.Model.Impl.Parsers
{
    public delegate IMetadataObject SingleComponentParser(PreprocessedProperty prop, IModelParserContext context);
    
    public class PropertyParsersMap
    {
        private readonly IModelPreParserContext _context;
        private readonly Dictionary<TypeMember, SingleComponentParser> _parserByPropType = 
            new Dictionary<TypeMember, SingleComponentParser>();

        public PropertyParsersMap(IModelPreParserContext context)
        {
            _context = context;
        }

        public void RegisterParsers(object container)
        {
            foreach (var method in container.GetType().GetMethods())
            {
                if (IsParserMethod(method, out var propType, out var parameterCount))
                {
                    // TODO: Reflection.Emit
                    SingleComponentParser parser = (prop, ctx) => {
                        Console.WriteLine($"- p: [{prop.Name} : {prop.Type.Name}] -> [{container.GetType().Name}::{method.Name}]");

                        var arguments = (
                            parameterCount == 1 
                            ? new object[] {prop} 
                            : new object[] {prop, ctx});
                        
                        var retVal = method.Invoke(
                            method.IsStatic ? null : container, 
                            arguments);
                        
                        return (IMetadataObject)retVal;
                    }; 
                    
                    _parserByPropType.Add(propType, parser);
                }
            }
        }


        public SingleComponentParser GetParser(PreprocessedProperty componentProp)
        {
            return (
                TryGetParser(componentProp) 
                ?? throw new BuildErrorException(
                    componentProp.Property,
                    $"Parser for property '{componentProp.Name}' of type '{componentProp.Type.FullName}' could not be found."));
        }

        public SingleComponentParser TryGetParser(PreprocessedProperty componentProp)
        {
            if (_parserByPropType.TryGetValue(componentProp.Type, out var parser))
            {
                return parser;
            }

            if (_context.Preprocessor.TryGetByConcreteType(componentProp.Type) is PreprocessedType propType)
            {
                return (prop, ctx) => ctx.GetMetadata(prop.Type);
            }

            var parseableBase = _parserByPropType.Keys
                .FirstOrDefault(k => k.IsAssignableFrom(componentProp.Type));

            if (!ReferenceEquals(parseableBase, null))
            {
                parser = _parserByPropType[parseableBase];
                _parserByPropType[componentProp.Type] = parser;
                return parser;
            }
            
            return null;
        }

        private bool IsParserMethod(MethodInfo method, out TypeMember propType, out int parameterCount)
        {
            var parameters = method.GetParameters();
            parameterCount = parameters.Length;
                
            var isParser = (
                method.IsPublic &&
                method.ReturnParameter != null &&
                typeof(IMetadataObject).IsAssignableFrom(method.ReturnParameter.ParameterType) &&
                (parameters.Length == 1 || parameters.Length == 2) &&
                parameters[0].ParameterType == typeof(PreprocessedProperty) &&
                (parameters.Length == 1 || parameters[1].ParameterType == typeof(IModelParserContext)));

            var propClrType = (isParser ? TryGetComponentType(method.ReturnParameter.ParameterType) : null); 
            propType = (isParser ? _context.Code.TryGetClrTypeMember(propClrType) : null);
            
            return (isParser && propType != null);
        }
        
        private Type TryGetComponentType(Type metadataType)
        {
            var metadataOf = metadataType
                .GetInterfaces()
                .FirstOrDefault(intf => intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IMetadataOf<>));

            return metadataOf?.GetGenericArguments()[0];
        }
    }
}

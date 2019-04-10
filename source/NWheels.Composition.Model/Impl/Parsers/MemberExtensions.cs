using System;
using System.Linq;
using MetaPrograms;
using MetaPrograms.CSharp;
using MetaPrograms.Members;
using MetaPrograms.Statements;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NWheels.Composition.Model.Impl.Parsers
{
    public static class MemberExtensions
    {
        public static T TryGetPropertyValue<T>(this TypeMember type, string propertyName)
        {
            var property = type.Members
                .OfType<PropertyMember>()
                .FirstOrDefault(p => p.Name == propertyName);

            var propValue =
                (property?.Getter?.Body.Statements.FirstOrDefault() as ReturnStatement)?.Expression;

            if (propValue != null && propValue.TryGetConstantValue(out var value))
            {
                return (T)value;
            }

            return default;
        }

        public static Type TryGetClrType(this TypeMember type)
        {
            if (type.TryGetBinding<Type>() is Type systemType)
            {
                return systemType;
            }

            if (type.TryGetBinding<SystemTypeNameBinding>() is SystemTypeNameBinding typeName)
            {
                return Type.GetType(typeName.SystemTypeMetadataName);
            }

            return null;
        }

        public static Type GetClrTypeOrThrow(this TypeMember type)
        {
            return TryGetClrType(type)
               ?? throw new BuildErrorException(
                   type,
                   $"Could not find CLR type for '{type.FullName}'");
        }
    }
}
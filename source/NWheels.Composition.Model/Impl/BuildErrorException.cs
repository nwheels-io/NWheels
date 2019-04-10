using System;
using System.Linq;
using System.Runtime.Serialization;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Composition.Model.Impl
{
    [Serializable]
    public class BuildErrorException : Exception
    {
        public BuildErrorException()
        {
        }
        
        public BuildErrorException(string message)
            : base(message)
        {
        }

        public BuildErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public BuildErrorException(
            AbstractMember member,
            string message, 
            Exception innerException = null) 
            : base(FormatMemberMessage(member, message), innerException)
        {
        }

        public BuildErrorException(
            IMetadataObject metaObject,
            string message, 
            Exception innerException = null) 
            : this(metaObject.Header.SourceType.ConcreteType, message, innerException)
        {
        }

        protected BuildErrorException(
            SerializationInfo info,
            StreamingContext context) 
            : base(info, context)
        {
        }

        public static string FormatMemberMessage(AbstractMember member, string message)
        {
            var location = member.TryGetBinding<ISymbol>()
                ?.DeclaringSyntaxReferences
                .Select(syntaxRef => syntaxRef.GetSyntax())
                .FirstOrDefault(x => x != null)
                ?.GetLocation()
                ?.GetLineSpan();

            var locationText = (
                location.HasValue 
                ? $"{location.Value.Path} ({location.Value.StartLinePosition.Line}:{location.Value.StartLinePosition.Character}) " 
                : string.Empty);

            return $"{locationText}ERROR: {message}";
        }
    }
}

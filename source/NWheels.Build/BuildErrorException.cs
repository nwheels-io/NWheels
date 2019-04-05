using System;
using System.Linq;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;

namespace NWheels.Build
{
    public class BuildErrorException : Exception
    {
        public BuildErrorException(
            AbstractMember member,
            string message, 
            Exception innerException = null) 
            : base(FormatMemberMessage(member, message), innerException)
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

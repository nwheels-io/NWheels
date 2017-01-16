using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using System.Linq;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class SourceCodeGenerator
    {
        public CompilationUnitSyntax GenerateSyntax(IEnumerable<TypeMember> typesToCompile)
        {
            var unitMemberSyntaxes = new List<MemberDeclarationSyntax>();

            EmitTypeSyntaxesGroupedInNamespaces(typesToCompile, unitMemberSyntaxes);

            var unitSyntax = 
                CompilationUnit()
                    .WithMembers(List(unitMemberSyntaxes));

            return unitSyntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EmitTypeSyntaxesGroupedInNamespaces(IEnumerable<TypeMember> typesToCompile, List<MemberDeclarationSyntax> unitMemberSyntaxes)
        {
            var typesGroupedByNamespace = typesToCompile
                .GroupBy(t => t.Namespace ?? string.Empty)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var singleNamespace in typesGroupedByNamespace)
            {
                var typeSyntaxes = new List<MemberDeclarationSyntax>();

                foreach (var type in singleNamespace)
                {
                    var typeSyntax = EmitTypeSyntax(type);
                    typeSyntaxes.Add(typeSyntax);
                }

                if (string.IsNullOrEmpty(singleNamespace.Key))
                {
                    unitMemberSyntaxes.AddRange(typeSyntaxes);
                }
                else
                {
                    var namespaceSyntax =
                        NamespaceDeclaration(ParseName(singleNamespace.Key))
                            .WithMembers(List(typeSyntaxes));

                    unitMemberSyntaxes.Add(namespaceSyntax);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax EmitTypeSyntax(TypeMember type)
        {
            MemberDeclarationSyntax typeSyntax;
            switch (type.TypeKind)
            {
                case TypeMemberKind.Class:
                    var enumEmitter = new ClassSyntaxEmitter(type);
                    typeSyntax = enumEmitter.EmitSyntax();
                    break;
                case TypeMemberKind.Enum:
                    var classEmitter = new EnumSyntaxEmitter(type);
                    typeSyntax = classEmitter.EmitSyntax();
                    break;
                default:
                    throw new NotSupportedException($"TypeMember of kind '{type.TypeKind}' is not supported.");
            }

            return typeSyntax;
        }
    }
}

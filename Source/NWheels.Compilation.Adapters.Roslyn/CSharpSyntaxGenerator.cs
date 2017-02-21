using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class CSharpSyntaxGenerator
    {
        public SyntaxTree GenerateSyntax(IEnumerable<TypeMember> typesToCompile, IReadOnlyCollection<TypeMember> allReferencedTypes = null)
        {
            var namespaceUsingSyntaxes = GatherNamespaceUsings(allReferencedTypes);
            var unitMemberSyntaxes = new List<MemberDeclarationSyntax>();

            EmitTypeSyntaxesGroupedInNamespaces(typesToCompile, unitMemberSyntaxes);

            var compilationUnitSyntax = 
                CompilationUnit()
                    .WithUsings(List(namespaceUsingSyntaxes))
                    .WithMembers(List(unitMemberSyntaxes))
                    .NormalizeWhitespace();

            var syntaxTree = CSharpSyntaxTree.Create(compilationUnitSyntax, encoding: Encoding.UTF8);

            return syntaxTree;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private UsingDirectiveSyntax[] GatherNamespaceUsings(IReadOnlyCollection<TypeMember> allReferencedTypes)
        {
            if (allReferencedTypes == null)
            {
                return new UsingDirectiveSyntax[0];
            }

            var namespacesToImport = new HashSet<string>();
            var typesGroupedByName = allReferencedTypes.GroupBy(t => t.Name);

            foreach (var identicalNameGroup in typesGroupedByName)
            {
                var identicalNameTypes = identicalNameGroup.Take(2).ToArray();
                if (identicalNameTypes.Length == 1 && !string.IsNullOrEmpty(identicalNameTypes[0].Namespace))
                {
                    namespacesToImport.Add(identicalNameTypes[0].Namespace);
                    identicalNameTypes[0].SafeBackendTag().IsNamespaceImported = true;
                }
            }

            var sortedNamespacesToImport = new List<string>(namespacesToImport);
            sortedNamespacesToImport.Sort(new NamespaceImportComparer());

            return sortedNamespacesToImport.Select(ns => UsingDirective(ParseName(ns))).ToArray();
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
                    var classEmitter = new ClassSyntaxEmitter(type);
                    typeSyntax = classEmitter.EmitSyntax();
                    break;
                case TypeMemberKind.Enum:
                    var enumEmitter = new EnumSyntaxEmitter(type);
                    typeSyntax = enumEmitter.EmitSyntax();
                    break;
                default:
                    throw new NotSupportedException($"TypeMember of kind '{type.TypeKind}' is not supported.");
            }

            var annotation = new SyntaxAnnotation();

            typeSyntax = typeSyntax.WithAdditionalAnnotations(annotation);
            type.SafeBackendTag().Annotation = annotation;

            return typeSyntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NamespaceImportComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var xIsSystem = x.StartsWith("System.");
                var yIsSystem = y.StartsWith("System.");

                if (xIsSystem && !yIsSystem)
                {
                    return -1;
                }

                if (yIsSystem && !xIsSystem)
                {
                    return 1;
                }

                return x.CompareTo(y);
            }
        }
    }
}

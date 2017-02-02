using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public static class MethodSignatureSyntaxEmitter
    {
        public static ParameterListSyntax EmitParameterListSyntax(MethodSignature signature)
        {
            return ParameterList(SeparatedList<ParameterSyntax>(signature.Parameters.Select(EmitParameterSyntax)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ParameterSyntax EmitParameterSyntax(MethodParameter parameter)
        {
            var syntax = Parameter(Identifier(parameter.Name));

            switch (parameter.Modifier)
            {
                case MethodParameterModifier.Ref:
                    syntax = syntax.WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)));
                    break;
                case MethodParameterModifier.Out:
                    syntax = syntax.WithModifiers(TokenList(Token(SyntaxKind.OutKeyword)));
                    break;
            }

            syntax = syntax.WithType(parameter.Type.GetTypeNameSyntax());

            return syntax;
        }
    }
}

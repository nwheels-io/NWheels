using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public static class AttributeSyntaxEmitter
    {
        public static AttributeSyntax EmitSyntax(AttributeDescription description)
        {
            var syntax = Attribute(SyntaxHelpers.GetTypeFullNameSyntax(description.AttributeType));

            if (description.ConstructorArguments.Count > 0 || description.PropertyValues.Count > 0)
            {
                syntax = syntax
                    .WithArgumentList(
                        AttributeArgumentList(
                            SeparatedList<AttributeArgumentSyntax>(
                                description.ConstructorArguments.Select(arg =>
                                    AttributeArgument(SyntaxHelpers.GetLiteralSyntax(arg)))
                                .Concat(description.PropertyValues.Select(pv =>
                                    AttributeArgument(ExpressionSyntaxEmitter.EmitSyntax(pv.Value))
                                    .WithNameEquals(NameEquals(IdentifierName(pv.Name)))
                                ))
                            )
                        )
                    );
            }

            return syntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
    }
}

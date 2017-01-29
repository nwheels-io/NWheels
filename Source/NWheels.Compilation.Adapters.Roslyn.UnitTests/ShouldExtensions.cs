using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public static class ShouldExtensions
    {
        public static SyntaxAssertions Should(this SyntaxTree actualValue)
        {
            return new SyntaxAssertions(actualValue.GetRoot());
        }

        public static SyntaxAssertions Should(this SyntaxNode actualValue)
        {
            return new SyntaxAssertions(actualValue);
        }

        private static string NormalizeCode(string code)
        {
            var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Script);
            var syntax = CSharpSyntaxTree.ParseText(code, options: parseOptions);
            var normalizedSyntax = syntax.GetRoot().NormalizeWhitespace(); //CSharpSyntaxTree.Create((CSharpSyntaxNode)syntax.GetRoot().NormalizeWhitespace(elasticTrivia: false));
            return normalizedSyntax.ToFullString();// GetText().ToString();
        }

        public class SyntaxAssertions
        {
            public SyntaxAssertions(SyntaxNode subject)
            {
                this.Subject = subject;
            }

            public AndConstraint<StringAssertions> BeEquivalentToCode(string expectedCode, string because = "", params object[] becauseArgs)
            {
                var normalizedActualCode = Subject.NormalizeWhitespace().GetText().ToString();
                var normalizedExpectedCode = NormalizeCode(expectedCode);
                return normalizedActualCode.Should().Be(normalizedExpectedCode, because, becauseArgs);
            }

            public SyntaxNode Subject { get; }
        }
    }
}

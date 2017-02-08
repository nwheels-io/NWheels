using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public static class StatementSyntaxEmitter
    {
        public static StatementSyntax EmitSyntax(AbstractStatement statement)
        {
            if (statement is ReturnStatement statementReturn)
            {
                return ReturnStatement(ExpressionSyntaxEmitter.EmitSyntax(statementReturn.Expression));
            }
            if (statement is BlockStatement statementBlock)
            {
                return statementBlock.ToSyntax();
            }
            if (statement is ThrowStatement statementThrow)
            {
                return ThrowStatement(ExpressionSyntaxEmitter.EmitSyntax(statementThrow.Exception));
            }
            if (statement is ExpressionStatement statementExpression)
            {
                return ExpressionStatement(ExpressionSyntaxEmitter.EmitSyntax(statementExpression.Expression));
            }
            if (statement is VariableDeclarationStatement statementVariable)
            {
                return EmitLocalDeclarationSyntax(statementVariable);
            }

            //TODO: support other types of statements

            throw new NotSupportedException($"Syntax emitter is not supported for statement of type '{statement.GetType().Name}'.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static LocalDeclarationStatementSyntax EmitLocalDeclarationSyntax(VariableDeclarationStatement statement)
        {
            var variable = statement.Variable;

            var declaration = (variable.Type != null
                ? VariableDeclaration(variable.Type.GetTypeNameSyntax())
                : VariableDeclaration(IdentifierName("var")));

            var declarator = VariableDeclarator(Identifier(variable.Name));

            if (statement.InitialValue != null)
            {
                declarator = declarator.WithInitializer(EqualsValueClause(ExpressionSyntaxEmitter.EmitSyntax(statement.InitialValue)));
            }

            return LocalDeclarationStatement(declaration.WithVariables(SingletonSeparatedList(declarator)));
        }
    }
}

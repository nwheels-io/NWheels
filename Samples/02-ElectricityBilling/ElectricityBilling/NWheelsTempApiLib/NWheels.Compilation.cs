using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Compilation.CodeModel;

namespace NWheels
{
    namespace Compilation
    {
        public class CodeWriter
        {
            public Statement BEGIN(params Statement[] statements) { return new BlockStatement { Statements = statements.ToList() }; }
            public Statement RETURN(Expr retVal = null) { return new ReturnStatement { ReturnValue = retVal }; }
            public AwaitExpr AWAIT(PromiseExpr promise) { return new AwaitExpr { Promise = promise }; }
            public Statement THROW(Expr exception = null) { return new ThrowStatement { Exception = exception }; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IfStatementWriter IF(Expr condition) { return new IfStatementWriter(); }
            public IfStatementWriter IF(Expression<Func<bool>> condition) { return new IfStatementWriter(); }
            public PromiseExprWriter CALL<TTarget>(Expression<Func<TTarget, Task>> invocation) { return new PromiseExprWriter(); }
            public PromiseExprWriter CALL(Expr target, string memberName, params Expr[] arguments) { return new PromiseExprWriter(); }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract class StatementWriterBase
            {
                protected abstract Statement GetStatement();

                public static implicit operator Statement(StatementWriterBase x)
                {
                    return x.GetStatement();
                }
            }

            public class IfStatementWriter : StatementWriterBase
            {
                public ElseStatementWriter THEN(params Statement[] block) { return new ElseStatementWriter(); }
                protected override Statement GetStatement() { return new IfStatement(); }
            }

            public class ElseStatementWriter : IfStatementWriter
            {
                public ElseStatementWriter ELSE(params Statement[] block) { return new ElseStatementWriter(); }
                public ElseStatementWriter ELSE_IF(params Statement[] block) { return new ElseStatementWriter(); }
                public IfStatement END_IF(params Statement[] block) { return new IfStatement(); }
            }

            public class PromiseExprWriter
            {
                public PromiseExprWriter THEN(params Statement[] block) { return new PromiseExprWriter(); }
                public PromiseExprWriter THEN(Func<Expr, Statement> block) { return new PromiseExprWriter(); }
                public PromiseExprWriter THEN<TResult>(Func<Expr<TResult>, Statement> block) { return new PromiseExprWriter(); }
                public PromiseExprWriter CATCH<TFault>(Func<Expr<TFault>, Statement> block) { return new PromiseExprWriter(); }
                public PromiseExprWriter CATCH(TypeRef faultType, Func<Expr, Statement> block) { return new PromiseExprWriter(); }
                public PromiseExprWriter CATCH(params Statement[] block) { return new PromiseExprWriter(); }
                public PromiseExprWriter FINALLY(params Statement[] block) { return new PromiseExprWriter(); }

                public static implicit operator PromiseExpr(PromiseExprWriter x)
                {
                    return new PromiseExpr();
                }

                public static implicit operator Statement(PromiseExprWriter x)
                {
                    return new EvalStatement { Expression = x };
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CodeWriter<TContext> : CodeWriter
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        namespace CodeModel
        {
            public class TypeRef
            {
                public string TypeName { get; set; }
                public Type ClrType { get; set; }

                public static readonly TypeRef Void = new TypeRef();
            }

            public abstract class Statement
            {
            }

            public abstract class Expr
            {
                public TypeRef Type { get; set; }

                public T As<T>()
                {
                    return default(T);
                }

                public static implicit operator Statement(Expr x)
                {
                    return new EvalStatement { Expression = x };
                }
            }

            public class Expr<T> : Expr
            {
                public T Value { get; set; }

                public static implicit operator T(Expr<T> expr)
                {
                    return default(T);
                }
            }

            public enum Operator
            {
                UnaryLogicalNot,
                UnaryBitwiseNot,
                UnaryPrefixIncrement,
                UnaryPrefixDecrement,
                UnaryPostfixIncrement,
                UnaryPostfixDecrement,
                LogicalOr,
                LogicalAnd,
                BitwiseOr,
                BitwiseAnd,
                BitwiseXor,
                Equal,
                NotEqual,
                Greater,
                GreaterOrEqual,
                Less,
                LessOrEqual,
                Plus,
                Minus,
                Multiply,
                Divide
            }

            public class CastExpr : Expr
            {
                public Expr Expression { get; set; }
            }

            public class CastExpr<T> : CastExpr
            {
                public T Value { get; set; }
            }

            public class ConstantExpr : Expr
            {
                public object Value { get; set; }
            }

            public class UnaryExpr : Expr
            {
                public Operator Operator { get; set; }
                public Expr Operand { get; set; }
            }

            public class BinaryExpr : Expr
            {
                public Operator Operator { get; set; }
                public Expr Left { get; set; }
                public Expr Right { get; set; }
            }

            public class LocalExpr : Expr
            {
                public string Name { get; set; }
            }

            public class ParameterExpr : Expr
            {
                public string Name { get; set; }
            }

            public class MemberExpr : Expr
            {
                public Expr Target { get; set; }
                public TypeRef TargetType { get; set; }
                public string Name { get; set; }
            }

            public class AssignmentExpr : Expr
            {
                public Expr Destination { get; set; }
                public Expr Value { get; set; }
            }

            public class CallExpr : MemberExpr
            {
                public List<Expr> Arguments { get; set; }
            }

            public class NamedArgumentExpr : Expr
            {
                public string Name { get; set; }
                public Expr Value { get; set; }
            }

            public class IndexExpr : Expr
            {
                public Expr Target { get; set; }
                public Expr Index { get; set; }
            }

            public class NewObjectExpr : Expr
            {
                public List<Expr> ConstructorArguments { get; set; }
            }

            public class NewArrayExpr : Expr
            {
                public TypeRef ElementType { get; set; }
                public Expr Length { get; set; }
            }

            public class BlockStatement : Statement
            {
                public List<Statement> Statements { get; set; }
            }

            public class EvalStatement : Statement
            {
                public Expr Expression { get; set; }
            }

            public class ReturnStatement : Statement
            {
                public Expr ReturnValue { get; set; }
            }

            public class IfStatement : Statement
            {
                public Expr Condition { get; set; }
                public Statement Then { get; set; }
                public Statement Else { get; set; }
            }

            public abstract class LoopStatement : Statement
            {
                public Expr Condition { get; set; }
                public Statement Body { get; set; }
            }

            public class WhileStatement : LoopStatement
            {
            }

            public class DoWhileStatement : LoopStatement
            {
            }

            public class ForStatement : LoopStatement
            {
                public List<Expr> Initializers { get; set; }
                public List<Expr> Iterators { get; set; }
            }

            public class BreakStatement : Statement
            {
            }

            public class ContinueStatement : Statement
            {
            }

            public class ThrowStatement : Statement
            {
                public Expr Exception { get; set; }
            }

            public class TryStatement : Statement
            {
                public Statement TryBlock { get; set; }
                public List<CatchBlock> CatchBlocks { get; set; }
                public Statement FinallyBlock { get; set; }
            }

            public class CatchBlock
            {
                public Type ExceptionType { get; set; }
                public Expr ExceptionFilter { get; set; }
                public Statement Body { get; set; }
            }

            public class SwitchStatement : Statement
            {
                public Expr Expression { get; set; }
                public List<CaseBlock> CaseBlocks { get; set; }
                public Statement DefaultBlock { get; set; }
            }

            public class CaseBlock
            {
                public Expr Value { get; set; }
                public Expr Filter { get; set; }
                public Statement Body { get; set; }
                public bool Break { get; set; }
            }

            public class TernaryExpr : Expr
            {
                public Expr Conditon;
                public Expr First;
                public Expr Second;
            }

            public class PromiseExpr : Expr
            {
                public List<PromiseExpr> Continuations { get; set; }
            }

            public class ThenPromiseExpr : PromiseExpr
            {
                public PromiseExpr Promise { get; set; }
                public Statement Block { get; set; }
            }

            public class CatchPromiseExpr : PromiseExpr
            {
                public TypeRef ExceptionType { get; set; }
                public PromiseExpr Promise { get; set; }
                public Statement Block { get; set; }
            }

            public class FinallyPromiseExpr : PromiseExpr
            {
                public PromiseExpr Promise { get; set; }
                public Statement Block { get; set; }
            }

            public class AwaitExpr : Expr
            {
                public PromiseExpr Promise { get; set; }
            }
        }
    }
}

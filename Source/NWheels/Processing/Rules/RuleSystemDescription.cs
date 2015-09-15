using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.Processing.Rules.Core;
using NWheels.Processing.Rules.Impl;
using NWheels.Utilities;

namespace NWheels.Processing.Rules
{
    [DataContract(Namespace = DataContractNamespace, Name = "RuleSystem")]
    public class RuleSystemDescription
    {
        public const string DataContractNamespace = "NWheels.Processing.Rules";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleSystemDescription()
        {
            this.Domain = new DomainDescription();
            this.RuleSets = new List<RuleSetDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Dictionary<string, DomainObject> BuildObjectByIdNameLookup(IRuleEngineLogger logger)
        {
            var lookup = new Dictionary<string, DomainObject>();

            AddObjectsToLookup(this.Domain.Variables, lookup, logger);
            AddObjectsToLookup(this.Domain.Functions, lookup, logger);
            AddObjectsToLookup(this.Domain.Actions, lookup, logger);
            AddObjectsToLookup(this.Domain.MetaRules, lookup, logger);

            return lookup;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddObjectsToLookup(IEnumerable<DomainObject> source, Dictionary<string, DomainObject> destination, IRuleEngineLogger logger)
        {
            foreach ( var obj in source )
            {
                if ( destination.ContainsKey(obj.IdName) )
                {
                    throw logger.DuplicateDomainObjectName(obj.IdName, duplicate: obj.GetType(), existing: destination[obj.IdName].GetType());
                }

                destination.Add(obj.IdName, obj);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string IdName { get; set; }
        [DataMember]
        public DomainDescription Domain { get; set; }
        [DataMember]
        public IList<RuleSetDescription> RuleSets { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

	    [DataContract(Namespace = DataContractNamespace)]
	    public class DomainDescription
	    {
		    public DomainDescription()
		    {
			    this.Variables = new List<DomainVariable>();
			    this.Functions = new List<DomainFunction>();
			    this.Actions = new List<DomainAction>();
                this.MetaRules = new List<DomainMetaRule>();
		    }
				
			//----------------------------------------------------------------------------------------------------------------------
	
		    [DataMember]
		    public IList<DomainVariable> Variables { get; private set; }

		    [DataMember]
		    public IList<DomainFunction> Functions { get; private set; }

		    [DataMember]
		    public IList<DomainAction> Actions { get; private set; }

		    [DataMember]
		    public IList<DomainMetaRule> MetaRules { get; set; }
	    }

	    //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: looks like this one is redundant
        //[DataContract(Namespace = DataContractNamespace)]
        //public class RuleCollection
        //{
        //    [DataMember]
        //    public IList<RuleSet> RuleSets { get; set; }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public abstract class DomainObject
        {
            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public string Description { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public abstract class DomainValueObject : DomainObject
        {
            [DataMember]
            public TypeDescription ValueType { get; set; }
            [DataMember]
            public List<string> StandardValues { get; set; }
            [DataMember]
            public bool StandardValuesExclusive { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class TypeDescription
        {
            public Type ToClrType()
            {
                return Type.GetType(this.TypeString, throwOnError: true);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string TypeString { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TypeDescription Of(Type type)
            {
                return new TypeDescription() {
                    TypeString = type.AssemblyQualifiedNameNonVersioned()
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TypeDescription Of<T>()
            {
                return Of(typeof(T));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class DomainVariable : DomainValueObject
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class DomainFunction : DomainValueObject
        {
            [DataMember]
            public IList<ParameterDescription> Parameters { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class DomainAction : DomainObject
        {
            public Type GetDelegateType(Type dataContextType)
            {
                var actionContextType = typeof(IRuleActionContext<>).MakeGenericType(dataContextType);
                var parameterTypes = Parameters.Select(p => p.Type.ToClrType());
                var typeArguments = new Type[] { actionContextType }.Concat(parameterTypes).ToArray();

                switch ( Parameters.Count)
                {
                    case 0:
                        return typeof(Action<>).MakeGenericType(typeArguments);
                    case 1:
                        return typeof(Action<,>).MakeGenericType(typeArguments);
                    case 2:
                        return typeof(Action<,,>).MakeGenericType(typeArguments);
                    case 3:
                        return typeof(Action<,,,>).MakeGenericType(typeArguments);
                    case 4:
                        return typeof(Action<,,,,>).MakeGenericType(typeArguments);
                    case 5:
                        return typeof(Action<,,,,,>).MakeGenericType(typeArguments);
                    case 6:
                        return typeof(Action<,,,,,,>).MakeGenericType(typeArguments);
                    case 7:
                        return typeof(Action<,,,,,,,>).MakeGenericType(typeArguments);
                    case 8:
                        return typeof(Action<,,,,,,,,>).MakeGenericType(typeArguments);
                }

                throw new NotSupportedException("Actions can have up to 8 parameters");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            [DataMember]
            public IList<ParameterDescription> Parameters { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class DomainMetaRule : DomainObject
        {
            [DataMember]
            public IList<string> AllowedRuleSetIds { get; set; }
            [DataMember]
            public IList<string> DisallowedRuleSetIds { get; set; }
            [DataMember]
            public IList<ParameterDescription> Parameters { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class ParameterDescription
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public TypeDescription Type { get; set; }
            [DataMember]
            public string Description { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum RuleSetMode
        {
            ApplyFirstMatch,
            ApplyAllMatches
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class RuleSetDescription
        {
            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public string Description { get; set; }
            [DataMember]
            public RuleSetMode Mode { get; set; }
            [DataMember]
            public Operand Precondition { get; set; }
            [DataMember]
            public bool FailIfNotMatched { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class RuleSet
        {
            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public IList<Rule> Rules { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class Rule
        {
            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public string Description { get; set; }
            [DataMember]
            public Operand Condition { get; set; }
            [DataMember]
            public IList<ActionInvocation> Actions { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class ActionInvocation
        {
            public void InvokeLateBound<TDataContext>(
                DomainAction objectDescription, 
                IRuleAction runtimeObject, 
                EvaluationContext<TDataContext> evaluationContext,
                IRuleActionContext<TDataContext> actionContext)
            {
                var delegateType = objectDescription.GetDelegateType(typeof(TDataContext));
                var delegateInstance = Delegate.CreateDelegate(delegateType, runtimeObject, "Apply", ignoreCase: false, throwOnBindFailure: true);
                
                var arguments = new object[Parameters.Count + 1];
                arguments[0] = actionContext;

                for ( int i = 0 ; i < Parameters.Count ; i++ )
                {
                    arguments[i + 1] = Parameters[i].Evaluate(evaluationContext);
                }

                delegateInstance.DynamicInvoke(arguments);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public IList<Operand> Parameters { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public abstract class Operand
        {
            public abstract object Evaluate<TDataContext>(EvaluationContext<TDataContext> context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class VariableOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                var runtimeObject = context.RuntimeObjectByIdName[this.IdName];
                var description = (DomainVariable)context.ObjectDescriptionByIdName[this.IdName];
                
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(TDataContext), description.ValueType.ToClrType());
                var delegateInstance = Delegate.CreateDelegate(delegateType, runtimeObject, "GetValue", ignoreCase: false, throwOnBindFailure: true);

                var value = delegateInstance.DynamicInvoke(context.DataContext);
                return value;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string IdName { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class FunctionOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                var function = (IRuleFunction)context.RuntimeObjectByIdName[this.IdName];
                var argumentValues = this.Arguments.Select(arg => arg.Evaluate(context)).ToArray();
                return function.GetValue(argumentValues);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public IList<Operand> Arguments { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class ConstantOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                var clrType = this.Type.ToClrType();
                return ParseUtility.Parse(this.ValueString, clrType);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public TypeDescription Type { get; set; }
            [DataMember]
            public string ValueString { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ConstantOperand FromValue<T>(T value)
            {
                return new ConstantOperand {
                    Type = TypeDescription.Of<T>(),
                    ValueString = value.ToString()
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class ListOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                return Items.Select(item => item.Evaluate(context)).ToArray();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public IList<Operand> Items { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class IntervalOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                var lowBoundValue = LowBound.Evaluate(context);
                var highBoundValue = HighBound.Evaluate(context);

                var intervalClosedType = typeof(Interval<>).MakeGenericType(lowBoundValue.GetType());
                var arguments = new object[] { lowBoundValue, highBoundValue, LowBoundType, HighBoundType };
                var intervalInstance = Activator.CreateInstance(intervalClosedType, arguments);

                return intervalInstance;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public Operand LowBound { get; set; }
            [DataMember]
            public Operand HighBound { get; set; }
            [DataMember]
            public IntervalType LowBoundType { get; set; }
            [DataMember]
            public IntervalType HighBoundType { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class DecisionTableOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                throw new NotImplementedException();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public IList<Operand> RowHeaders { get; set; }
            [DataMember]
            public IList<Operand> ColumnHeaders { get; set; }
            [DataMember]
            public IList<IList<DecisionTableHeaderCell>> RowHeaderCells { get; set; }
            [DataMember]
            public IList<IList<DecisionTableHeaderCell>> ColumnHeaderCells { get; set; }
            [DataMember]
            public IList<IList<Operand>> BodyCells { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DecisionTableHeaderCell
        {
            [DataMember]
            public Operator Operator { get; set; }
            [DataMember]
            public Operand Value { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class RuleSetOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                throw new NotImplementedException();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public RuleSet Rules { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public abstract class ExpressionOperand : Operand
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class UnaryExpressionOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                var operandValue = Operand.Evaluate(context);

                switch ( Operator )
                {
                    case Operator.LogicalNot:
                        return EvaluationHelpers.EvaluateLogicalNot(operandValue);
                    case Operator.Negation:
                        return EvaluationHelpers.EvaluateNegation(operandValue);
                }

                throw new NotSupportedException("Unary operator not supported: " + Operator.ToString());
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public Operand Operand { get; set; }
            [DataMember]
            public Operator Operator { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class BinaryExpressionOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                var leftValue = Left.Evaluate(context);
                var rightValue = Right.Evaluate(context);

                switch ( Operator )
                {
                    case Operator.Equal:
                        return EvaluationHelpers.EvaluateEqual(leftValue, rightValue);
                    case Operator.NotEqual:
                        return EvaluationHelpers.EvaluateNotEqual(leftValue, rightValue);
                    case Operator.GreaterThan:
                        return EvaluationHelpers.EvaluateGreaterThan(leftValue, rightValue);
                    case Operator.GreaterThanOrEqual:
                        return EvaluationHelpers.EvaluateGreaterThanOrEqual(leftValue, rightValue);
                    case Operator.LessThan:
                        return EvaluationHelpers.EvaluateLessThan(leftValue, rightValue);
                    case Operator.LessThanOrEqual:
                        return EvaluationHelpers.EvaluateLessThanOrEqual(leftValue, rightValue);
                    case Operator.In:
                        return EvaluationHelpers.EvaluateIn(leftValue, rightValue);
                    case Operator.LogicalAnd:
                        return EvaluationHelpers.EvaluateLogicalAnd(leftValue, rightValue);
                    case Operator.LogicalOr:
                        return EvaluationHelpers.EvaluateLogicalOr(leftValue, rightValue);
                    case Operator.Add:
                        return EvaluationHelpers.EvaluateAdd(leftValue, rightValue);
                    case Operator.Subtract:
                        return EvaluationHelpers.EvaluateSubtract(leftValue, rightValue);
                    case Operator.Multiply:
                        return EvaluationHelpers.EvaluateMultiply(leftValue, rightValue);
                    case Operator.Divide:
                        return EvaluationHelpers.EvaluateDivide(leftValue, rightValue);
                    case Operator.Modulo:
                        return EvaluationHelpers.EvaluateModulo(leftValue, rightValue);
                    case Operator.Percentage:
                        return EvaluationHelpers.EvaluatePercentage(leftValue, rightValue);
                    case Operator.Min:
                        return EvaluationHelpers.EvaluateMin(leftValue, rightValue);
                    case Operator.Max:
                        return EvaluationHelpers.EvaluateMax(leftValue, rightValue);
                    case Operator.Average:
                        return EvaluationHelpers.EvaluateAverage(leftValue, rightValue);
                    case Operator.Coalesce:
                        return EvaluationHelpers.EvaluateCoalesce(leftValue, rightValue);
                }

                throw new NotSupportedException("Binary operator not supported: " + Operator.ToString());
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public Operand Left { get; set; }
            [DataMember]
            public Operator Operator { get; set; }
            [DataMember]
            public Operand Right { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class LogicalTernaryOperand : Operand
        {
            #region Overrides of Operand

            public override object Evaluate<TDataContext>(EvaluationContext<TDataContext> context)
            {
                if ( (bool)Condition.Evaluate(context) == true )
                {
                    return OnTrue.Evaluate(context);
                }
                else
                {
                    return OnFalse.Evaluate(context);
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public Operand Condition { get; set; }
            [DataMember]
            public Operand OnTrue { get; set; }
            [DataMember]
            public Operand OnFalse { get; set; }

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public enum Operator
        {
            Equal,
            NotEqual,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
            In,  // both IN LIST and IN RANGE
            LogicalAnd,
            LogicalOr,
            LogicalNot,
            Add,
            Subtract,
            Multiply,
            Divide,
            Negation,
            Modulo,
            Percentage, // X PERCENTAGE Y means X percents of Y
            Min,
            Max,
            Average,
            Coalesce
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum StandardVariableName
        {
            Now,
            TimeOfDay,
            DayOfWeek,
            WeekOfYear,
            DayOfYear,
            UtcNow,
            UtcTimeOfDay,
            UtcDayOfWeek,
            UtcWeekOfYear,
            UtcDayOfYear,
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum StandardFunctionName
        {
            Abs,    
            Acos,   
            Atan,
            Atan2,
            Ceil,
            Cos,
            Cosh,
            Exp,
            Floor,
            Log,
            Log2,
            Log10,
            Power,
            Round,
            Sign,
            Sin,
            Sinh,
            Sqrt,
            Tan,
            Tanh,
            Truncate,
            Weeks,
            Days,
            Hours,
            Minutes,
            AddWeeks,
            AddDays,
            AddHours,
            AddMinutes,
        }
    }
}

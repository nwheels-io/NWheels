using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NWheels.Extensions;

namespace NWheels.Processing.Rules
{
    [DataContract(Namespace = DataContractNamespace, Name = "RuleSystem")]
    public class RuleSystemDescription
    {
        public const string DataContractNamespace = "NWheels.Processing.Rules";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

        [DataContract(Namespace = DataContractNamespace)]
        public class RuleCollection
        {
            [DataMember]
            public IList<RuleSet> RuleSets { get; set; }
        }

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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class TypeDescription
        {
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
            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public IList<Operand> Parameters { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public abstract class Operand
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class VariableOperand : Operand
        {
            [DataMember]
            public string IdName { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class FunctionOperand : Operand
        {
            [DataMember]
            public string IdName { get; set; }
            [DataMember]
            public IList<Operand> Arguments { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class ConstantOperand : Operand
        {
            [DataMember]
            public TypeDescription Type { get; set; }
            [DataMember]
            public string ValueString { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class ListOperand : Operand
        {
            [DataMember]
            public IList<Operand> Items { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class RangeOperand : Operand
        {
            [DataMember]
            public Operand LowBound { get; set; }
            [DataMember]
            public Operand HighBound { get; set; }
            [DataMember]
            public bool LowBoundExclusive { get; set; }
            [DataMember]
            public bool HighBoundExclusive { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class DecisionTableOperand : Operand
        {
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
            [DataMember]
            public Operand Operand { get; set; }
            [DataMember]
            public Operator Operator { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = DataContractNamespace)]
        public class BinaryExpressionOperand : Operand
        {
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

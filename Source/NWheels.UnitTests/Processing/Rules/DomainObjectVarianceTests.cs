using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Processing.Rules.Core;

namespace NWheels.UnitTests.Processing.Rules
{
    [TestFixture]
    public class DomainObjectVarianceTests
    {
        [Test]
        public void CanUseInlineBaseContextVariableWithDerivedContext()
        {
            //-- arrange

            var stringVariable = new RuleVariable<BaseContext, string>(
                context => context.StringValue,
				()=> new List<object>(), 
                idName: "StringValue",
                description: "StringValue from base context");

            var contextOne = new DerivedContextOne() {
                StringValue = "ABC"
            };
            
            var contextTwo = new DerivedContextTwo() {
                StringValue = "DEF"
            };

            //-- act

            var value1 = stringVariable.GetValue(contextOne); // because DerivedContextOne inherits from BaseContext
            var value2 = stringVariable.GetValue(contextTwo); // because DerivedContextTwo inherits from BaseContext

            //-- assert

            Assert.That(value1, Is.EqualTo("ABC"));
            Assert.That(value2, Is.EqualTo("DEF"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCastInlineBaseContextVariableToDerivedContextType()
        {
            //-- arrange

            IRuleVariable<BaseContext, string> baseVariable = new RuleVariable<BaseContext, string>(
                context => context.StringValue,
				() => new List<object>(), 
                idName: "StringValue",
                description: "StringValue from base context");

            // the following casts are allowed because of co/contra-variance feature of C# 5:
            // (1) IRuleVariable declares TDataContext as contravariant: IRuleVariable<in TDataContext, ...>
            // (2) DerivedContextOne and DerivedContextTwo inherit BaseContext
            IRuleVariable<DerivedContextOne, string> baseVariableCastToDerivedContextOne = baseVariable;
            IRuleVariable<DerivedContextTwo, string> baseVariableCastToDerivedContextTwo = baseVariable;

            var contextOne = new DerivedContextOne() {
                StringValue = "ABC"
            };

            var contextTwo = new DerivedContextTwo() {
                StringValue = "DEF"
            };

            //-- act

            var value1 = baseVariableCastToDerivedContextOne.GetValue(contextOne);
            var value2 = baseVariableCastToDerivedContextTwo.GetValue(contextTwo);

            //-- assert

            Assert.That(value1, Is.EqualTo("ABC"));
            Assert.That(value2, Is.EqualTo("DEF"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCastVariableInterfaceToObjectResult()
        {
            //-- arrange

            IRuleVariable<BaseContext, string> baseVariable = new RuleVariable<BaseContext, string>(
                context => context.StringValue,
				() => new List<object>(), 
                idName: "StringValue",
                description: "StringValue from base context");

            // the following cast is allowed because of co/contra-variance feature of C# 5:
            // (1) IRuleVariable declares TValue as covariant: IRuleVariable<..., out TValue>
            // (2) Type 'object' is assignable from type 'string'
            IRuleVariable<BaseContext, object> variableWithObjectResult = baseVariable;

            var contextOne = new DerivedContextOne() {
                StringValue = "ABC"
            };

            //-- act

            object value = variableWithObjectResult.GetValue(contextOne);

            //-- assert

            Assert.That(value, Is.EqualTo("ABC"));
        }

		//----------------------------------------------------------------------------------------------------------------------

		[Test]
	    public void CanInventoryValues()
	    {
		    var variable = new RuleVariable<BaseContext, int>(
			    context=>1,
			    () =>  new List<object>() {
					    1,
					    2,
					    3,
					    4,
					    5,
					    6,
					    7
				    },
			    idName: "values",
			    description: "values");
		    var values = variable.GetInventoryValues();
			Assert.That(values.Count,Is.EqualTo(7));
			Assert.That(values[0],Is.EqualTo(1));
	    }

	    //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BaseContext
        {
            public string StringValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DerivedContextOne : BaseContext
        {
            public int IntValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DerivedContextTwo : BaseContext
        {
            public Money MoneyValue { get; set; }
        }
    }
}

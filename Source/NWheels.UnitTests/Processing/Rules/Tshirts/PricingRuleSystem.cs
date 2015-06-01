using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Rules.Core;
using NWheels.UI.Globalization;

namespace NWheels.UnitTests.Processing.Rules.Tshirts
{
    public class PricingRuleSystem : IRuleSystemCodeBehind<PricingContext>
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PricingRuleSystem(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleSystemCodeBehind<PricingContext>

        public void BuildRuleSystem(RuleSystemBuilder<PricingContext> builder)
        {
            builder.AddVariable(context => context.Current.Color,()=> new List<object>(),  "Color", "The color of the T-shirt");
            builder.AddVariable(context => context.Current.Size, ()=> new List<object>(),"Size", "The size of the T-shirt");
            builder.AddScalarPropertiesAsVariables(context => context.Current.Model);
            builder.AddAction(new AddPriceLineAction());
            // (1) all this (context+variables+functions+actions) builds RuleSystemDescription
            // (2) RuleSystemDescription + RuleSystemData = CompiledRuleSystem
            // (3) CompiledRuleSystem.Run(context)

            // CUSTOMER BIRTHDAY - VARIANT 1
            //builder.AddVariable(context => context.Customer.Birthday == _framework.UtcNow.Date, "IsCustomerBirthday", "True if today is customer's birthday, False otherwise.");

            // CUSTOMER BIRTHDAY - VARIANT 2
            builder.AddVariable(new IsCustomerBirthdayToday(_framework));

            // CUSTOMER BIRTHDAY - VARIANT 3
			//builder.AddVariable(new RuleVariable<PricingContext, bool>(
			//	context => context.Customer.Birthday == _framework.UtcNow.Date, 
			//	"IsCustomerBirthday", 
			//	"True if today is customer's birthday, False otherwise."));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class IsCustomerBirthdayToday : IRuleVariable<CustomerContextBase, bool>
        {
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IsCustomerBirthdayToday(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool GetValue(CustomerContextBase context)
            {
	            var utcBirthday = context.Customer.Birthday.ToUniversalTime();
                return ( utcBirthday.Day  == _framework.UtcNow.Date.Day  && utcBirthday.Month ==_framework.UtcNow.Date.Month);
            }
            public string IdName
            {
                get { return "IsCustomerBirthday"; }
            }
            public string Description
            {
                get { return "True if today is customer's birthday, False otherwise."; }
            }

			#region IRuleVariable<CustomerContextBase,bool> Members


			public IList<object> GetInventoryValues()
			{
				return new List<object>();
			}

			#endregion
		}


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AddPriceLineAction : RuleActionBase<PricingContext, Money, string>
        {
            public AddPriceLineAction()
                : base("AddPriceLine", description: "Add price line")
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of RuleActionBase<PricingContext,Money,string>

            public override void Apply(IRuleActionContext<PricingContext> context, Money price, string description)
            {
                context.Data.PriceLines.Add(new PricingContext.PriceLine() {
                    Price = price,
                    Description = description
                });
            }

            #endregion
        }
    }
}

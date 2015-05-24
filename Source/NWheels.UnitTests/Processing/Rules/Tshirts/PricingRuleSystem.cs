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
            builder.AddVariable(context => context.Current.Color, "Color", "The color of the T-shirt");
            builder.AddVariable(context => context.Current.Size, "Size", "The size of the T-shirt");
            builder.AddScalarPropertiesAsVariables(context => context.Current.Model);
            builder.AddAction(new AddPriceLineAction());
            // (1) all this (context+variables+functions+actions) builds RuleSystemDescription
            // (2) RuleSystemDescription + RuleSystemData = CompiledRuleSystem
            // (3) CompiledRuleSystem.Run(context)

            // CUSTOMER BIRTHDAY - VARIANT 1
            builder.AddVariable(context => context.Customer.Birthday == _framework.UtcNow.Date, "IsCustomerBirthday", "True if today is customer's birthday, False otherwise.");

            // CUSTOMER BIRTHDAY - VARIANT 2
            builder.AddVariable(new IsCustomerBirthdayToday(_framework));

            // CUSTOMER BIRTHDAY - VARIANT 3
            builder.AddVariable(new RuleVariable<PricingContext, bool>(
                context => context.Customer.Birthday == _framework.UtcNow.Date, 
                "IsCustomerBirthday", 
                "True if today is customer's birthday, False otherwise."));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class IsCustomerBirthdayToday : IRuleVariable<PricingContext, bool>
        {
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IsCustomerBirthdayToday(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool GetValue(PricingContext context)
            {
                return (context.Customer.Birthday == _framework.UtcNow.Date);
            }
            public string IdName
            {
                get { return "IsCustomerBirthday"; }
            }
            public string Description
            {
                get { return "True if today is customer's birthday, False otherwise."; }
            }
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

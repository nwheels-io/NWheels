using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Rules.Core;

namespace NWheels.UnitTests.Processing.Rules
{
    public class TestCodeBehind : IRuleSystemCodeBehind<TestDataContext>
    {
        #region Implementation of IRuleSystemCodeBehind<TestCodeBehind>

        public void BuildRuleSystem(RuleSystemBuilder<TestDataContext> builder)
        {
            builder.AddVariable(x => x.Input, "Input");
            builder.AddVariable(x => (x.Input % 2) == 0, "Even");
            builder.AddVariable(x => (Math.Abs(x.Input) / 100) + 1, "AbsoluteHundred");
            
            builder.AddAction(x => x.ActionLog.Add("ActionPositive"), "ActionPositive");
            builder.AddAction(x => x.ActionLog.Add("ActionNegative"), "ActionNegative");
            builder.AddAction(x => x.ActionLog.Add("ActionZero"), "ActionZero");
            builder.AddAction(x => x.ActionLog.Add("ActionEven"), "ActionEven");
            builder.AddAction(x => x.ActionLog.Add("ActionOdd"), "ActionOdd");
            builder.AddAction(x => x.ActionLog.Add("ActionFirstHundred"), "ActionFirstHundred");
            builder.AddAction(x => x.ActionLog.Add("ActionSecondHundred"), "ActionSecondHundred");
            builder.AddAction(x => x.ActionLog.Add("ActionThirdHundred"), "ActionThirdHundred");

            builder.AddRuleSet("RuleSetSign");
            builder.AddRuleSet("RuleSetEvenOdd");
            builder.AddRuleSet("RuleSetHundred");
        }

        #endregion
    }
}

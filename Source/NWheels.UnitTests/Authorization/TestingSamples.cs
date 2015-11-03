using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Authorization.Claims;
using NWheels.Entities;

namespace NWheels.UnitTests.Authorization
{
    public static class TestingSamples
    {
        public static Claim MakeRuleOneClaim()
        {
            return new EntityAccessRuleClaim(new RuleOne(), "R1");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Claim MakeRuleTwoClaim()
        {
            return new EntityAccessRuleClaim(new RuleTwo(), "R2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Claim MakeRuleThreeClaim()
        {
            return new EntityAccessRuleClaim(new RuleThree(), "R3");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class RuleBase : IEntityAccessRule
        {
            private Action<IEntityAccessControlBuilder> _onBuildAccessControl;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEntityAccessRule

            void IEntityAccessRule.BuildAccessControl(IEntityAccessControlBuilder builder)
            {
                BuildAccessControlCount++;

                if ( _onBuildAccessControl != null )
                {
                    _onBuildAccessControl(builder);
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnBuildAccessControl(Action<IEntityAccessControlBuilder> callback)
            {
                _onBuildAccessControl += callback;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int BuildAccessControlCount { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RuleOne : RuleBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RuleTwo : RuleBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RuleThree : RuleBase
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IEntityOne
        {
            int Id { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IEntityTwoBase
        {
            int Id { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IEntityTwoDerivativeA : IEntityTwoBase
        {
            int IntValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IEntityTwoDerivativeB : IEntityTwoBase
        {
            string StringValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IEntityTwoDerivativeBSub : IEntityTwoDerivativeB
        {
            TimeSpan TimeValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestDomainContext : IApplicationDataRepository
        {
            IEntityRepository<IEntityOne> Ones { get; }
            IEntityRepository<IEntityTwoBase> Twos { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEntityTwoBase NewTwoBase(int id);
            IEntityTwoDerivativeA NewTwoDerivativeA(int id, DayOfWeek dayValue);
            IEntityTwoDerivativeB NewTwoDerivativeB(int id, string stringValue);
            IEntityTwoDerivativeBSub NewTwoDerivativeBSub(int id, string stringValue, TimeSpan timeValue);
        }
    }
}

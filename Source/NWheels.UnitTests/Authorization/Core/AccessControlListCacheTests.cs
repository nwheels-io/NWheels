using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Authorization;
using NWheels.Authorization.Claims;
using NWheels.Authorization.Core;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Authorization.Core
{
    [TestFixture]
    public class AccessControlListCacheTests : UnitTestBase
    {
        [Test]
        public void CanCreateAccessControlListForSingleRule()
        {
            //- arrange

            var cacheUnderTest = new AccessControlListCache(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>());

            var claims = new Claim[] {
                TestingSamples.MakeRuleOneClaim(), 
            };

            //- act

            var acl = cacheUnderTest.GetAccessControlList(claims);

            //- assert

            acl.ShouldNotBeNull();
            acl.GetClaims().ShouldBe(claims);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateAccessControlListForTwoRules()
        {
            //- arrange

            var cacheUnderTest = new AccessControlListCache(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>());

            var claims = new Claim[] {
                TestingSamples.MakeRuleOneClaim(), 
                TestingSamples.MakeRuleTwoClaim(), 
            };

            //- act

            var acl = cacheUnderTest.GetAccessControlList(claims);

            //- assert

            acl.ShouldNotBeNull();
            acl.GetClaims().ShouldBe(claims);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCacheAccessControlListPerSetOfClaims()
        {
            //- arrange

            var cacheUnderTest = new AccessControlListCache(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>());

            var claimSet1 = new Claim[] {
                TestingSamples.MakeRuleOneClaim(), 
                TestingSamples.MakeRuleTwoClaim(), 
            };

            var claimSet2 = new Claim[] {
                TestingSamples.MakeRuleOneClaim(), 
                TestingSamples.MakeRuleTwoClaim(), 
            };

            var claimSet3 = new Claim[] {
                TestingSamples.MakeRuleThreeClaim(), 
            };

            //- act

            var acl1 = cacheUnderTest.GetAccessControlList(claimSet1);
            var acl2 = cacheUnderTest.GetAccessControlList(claimSet2);
            var acl3 = cacheUnderTest.GetAccessControlList(claimSet3);

            //- assert

            acl2.ShouldBeSameAs(acl1);
            acl3.ShouldNotBeSameAs(acl1);

            acl1.GetClaims().ShouldBe(claimSet1);
            acl3.GetClaims().ShouldBe(claimSet3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanIgnoreOrderOfClaimsInClaimSet()
        {
            //- arrange

            var cacheUnderTest = new AccessControlListCache(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>());

            var claimsOneAndTwo = new Claim[] {
                TestingSamples.MakeRuleOneClaim(), 
                TestingSamples.MakeRuleTwoClaim(), 
            };

            var claimsTwoAndOne = new Claim[] {
                TestingSamples.MakeRuleTwoClaim(), 
                TestingSamples.MakeRuleOneClaim(), 
            };

            //- act

            var aclOneAndTwo = cacheUnderTest.GetAccessControlList(claimsOneAndTwo);
            var aclTwoAndOne = cacheUnderTest.GetAccessControlList(claimsTwoAndOne);

            //- assert

            aclOneAndTwo.ShouldBeSameAs(aclTwoAndOne);
        }
    }
}

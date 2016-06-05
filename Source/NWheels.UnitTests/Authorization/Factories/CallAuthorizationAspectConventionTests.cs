using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Authorization.Factories;
using NWheels.Exceptions;
using NWheels.Hosting.Core;
using NWheels.Hosting.Factories;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Authorization.Factories
{
    [TestFixture]
    public class CallAuthorizationAspectConventionTests : DynamicTypeUnitTestBase
    {
        private ComponentAspectFactory _aspectFactory;
        private List<string> _log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            Framework.UpdateComponents(b => b.RegisterInstance(_log));

            var aspectPipeline = new Pipeline<IComponentAspectProvider>(new IComponentAspectProvider[] {
                new CallAuthorizationAspectConvention.AspectProvider(),
            });
            
            _aspectFactory = new ComponentAspectFactory(Framework.Components, base.DyamicModule, aspectPipeline);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ClassLevelCheck_NotSatisfied_Throw()
        {
            //-- arrange

            var component = (TestComponentOne)_aspectFactory.CreateInheritor(typeof(TestComponentOne));
            Thread.CurrentPrincipal = null;

            //-- act & assert

            var exception = Should.Throw<AccessDeniedException>(() => {
                component.DoSomething();
            });

            _log.ShouldBeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ClassLevelCheck_Satisfied_DoNotThrow()
        {
            //-- arrange

            var component = (TestComponentOne)_aspectFactory.CreateInheritor(typeof(TestComponentOne));
            Thread.CurrentPrincipal = new SecurityCheckTests.AuthenticatedPrincipal(userRoles: new[] { "Admin" });

            //-- act & assert

            Should.NotThrow(() => {
                component.DoSomething();
            });

            _log.ShouldBe(new[] { "TestComponentOne.DoSomething" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MethodLevelCheck_NotSatisfied_Throw()
        {
            //-- arrange

            var component = (TestComponentTwo)_aspectFactory.CreateInheritor(typeof(TestComponentTwo));
            Thread.CurrentPrincipal = null;

            //-- act & assert

            var exception = Should.Throw<AccessDeniedException>(() => {
                component.DoSomethingOnlyAdminIsAllowedTo();
            });

            _log.ShouldBeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MethodLevelCheck_Satisfied_DoNotThrow()
        {
            //-- arrange

            var component = (TestComponentTwo)_aspectFactory.CreateInheritor(typeof(TestComponentTwo));
            Thread.CurrentPrincipal = new SecurityCheckTests.AuthenticatedPrincipal(userRoles: new[] { "Admin" });

            //-- act & assert

            Should.NotThrow(() => {
                component.DoSomethingOnlyAdminIsAllowedTo();
            });

            _log.ShouldBe(new[] { "TestComponentTwo.DoSomethingOnlyAdminIsAllowedTo" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NoCheck_UserNotAuthenticated_Throw()
        {
            //-- arrange

            var component = (TestComponentTwo)_aspectFactory.CreateInheritor(typeof(TestComponentTwo));
            Thread.CurrentPrincipal = null;

            //-- act & assert

            Should.Throw<AccessDeniedException>(() => {
                component.DoSomethingThatRequiresLogin();
            });

            _log.ShouldBeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NoCheck_UserAuthenticated_DoNotThrow()
        {
            //-- arrange

            var component = (TestComponentTwo)_aspectFactory.CreateInheritor(typeof(TestComponentTwo));
            Thread.CurrentPrincipal = new SecurityCheckTests.AuthenticatedPrincipal();

            //-- act & assert

            Should.NotThrow(() => {
                component.DoSomethingThatRequiresLogin();
            });

            _log.ShouldBe(new[] { "TestComponentTwo.DoSomethingThatRequiresLogin" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ClassAndMethodLevelChecksCombined_MethodLevelOverrides()
        {
            //-- arrange

            var component = (TestComponentOne)_aspectFactory.CreateInheritor(typeof(TestComponentOne));
            Thread.CurrentPrincipal = null;

            //-- act & assert

            Should.NotThrow(() => {
                component.DoSomethingEveryoneIsAllowedTo();
            });

            _log.ShouldBe(new[] { "TestComponentOne.DoSomethingEveryoneIsAllowedTo" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SecurityCheck.RequireUserRole("Admin")]
        public class TestComponentOne
        {
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestComponentOne(List<string> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void DoSomething()
            {
                _log.Add("TestComponentOne.DoSomething");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            [SecurityCheck.AllowAnonymous]
            public virtual void DoSomethingEveryoneIsAllowedTo()
            {
                _log.Add("TestComponentOne.DoSomethingEveryoneIsAllowedTo");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestComponentTwo
        {
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestComponentTwo(List<string> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [SecurityCheck.RequireUserRole("Admin")]
            public virtual void DoSomethingOnlyAdminIsAllowedTo()
            {
                _log.Add("TestComponentTwo.DoSomethingOnlyAdminIsAllowedTo");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void DoSomethingThatRequiresLogin()
            {
                _log.Add("TestComponentTwo.DoSomethingThatRequiresLogin");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [SecurityCheck.AllowAnonymous]
            public virtual void DoSomethingEveryoneIsAllowedTo()
            {
                _log.Add("TestComponentTwo.DoSomethingEveryoneIsAllowedTo");
            }
        }
    }
}

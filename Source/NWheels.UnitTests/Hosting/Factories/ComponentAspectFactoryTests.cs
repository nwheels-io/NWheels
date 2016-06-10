using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.Core;
using NWheels.Extensions;
using NWheels.Hosting.Core;
using NWheels.Hosting.Factories;
using NWheels.Logging.Factories;
using NWheels.Testing;
using Shouldly;
using TT = Hapil.TypeTemplate;

namespace NWheels.UnitTests.Hosting.Factories
{
    [TestFixture]
    public class ComponentAspectFactoryTests : DynamicTypeUnitTestBase
    {
        private List<string> _log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            Framework.UpdateComponents(b => b.RegisterInstance(_log).As<List<string>>());

            CreateFactoryUnderTest();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_SingleAspect_Create()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var component = new TestComponentOne(_log);

            //-- act

            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- assert

            aspectProxy.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_MultipleAspects_Create()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);

            //-- act

            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- assert

            aspectProxy.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_MultipleInterfaces_Create()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentTwo(_log);

            //-- act

            var aspectProxy = factoryUnderTest.CreateProxy(component, new[] {
                typeof(ITestComponent), 
                typeof(ITestComponentTwo)
            });

            //-- assert

            aspectProxy.ShouldNotBeNull();
            aspectProxy.ShouldBeAssignableTo<ITestComponent>();
            aspectProxy.ShouldBeAssignableTo<ITestComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_SingleAspect_InvokeVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var component = new TestComponentOne(_log);
            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- act

            aspectProxy.MethodOne();

            //-- assert

            _log.ShouldBe(new[] {
                "Test:BEFORE:MethodOne", 
                "COMPONENT:MethodOne", 
                "Test:AFTER:MethodOne"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_MultipleAspects_InvokeVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);
            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- act

            aspectProxy.MethodOne();

            //-- assert

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodOne", 
                "AspectB:BEFORE:MethodOne", 
                "AspectC:BEFORE:MethodOne", 
                "COMPONENT:MethodOne", 
                "AspectC:AFTER:MethodOne",
                "AspectB:AFTER:MethodOne",
                "AspectA:AFTER:MethodOne",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_MultipleAspects_InvokeVoidMethodWithParameters()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB")
            );
            
            var component = new TestComponentTwo(_log);

            var aspectProxy = factoryUnderTest.CreateProxy(component, new[] {
                typeof(ITestComponent), 
                typeof(ITestComponentTwo)
            });

            //-- act

            ((ITestComponent)aspectProxy).MethodOne();
            ((ITestComponentTwo)aspectProxy).MethodThree();

            //-- assert

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodOne", 
                "AspectB:BEFORE:MethodOne", 
                "COMPONENT:MethodOne", 
                "AspectB:AFTER:MethodOne",
                "AspectA:AFTER:MethodOne",
                
                "AspectA:BEFORE:MethodThree", 
                "AspectB:BEFORE:MethodThree", 
                "COMPONENT:MethodThree", 
                "AspectB:AFTER:MethodThree",
                "AspectA:AFTER:MethodThree",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_SingleAspect_InvokeFunction()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var component = new TestComponentOne(_log);
            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectProxy.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABC");
            _log.ShouldBe(new[] {
                "Test:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "Test:AFTER:MethodTwo"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_MultipleAspects_InvokeFunction()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);
            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectProxy.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABC");
            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "AspectC:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "AspectC:AFTER:MethodTwo",
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_MultipleInterfaces_InvokeFunction()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB")
            );
            
            var component = new TestComponentTwo(_log);

            var aspectProxy = factoryUnderTest.CreateProxy(component, new[] {
                typeof(ITestComponent), 
                typeof(ITestComponentTwo)
            });

            //-- act

            var returnValue1 = ((ITestComponent)aspectProxy).MethodTwo(num: 123);
            var returnValue2 = ((ITestComponentTwo)aspectProxy).MethodFour(num: 456);

            //-- assert

            returnValue1.ShouldBe("ABC");
            returnValue2.ShouldBe("DEF");

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",

                "AspectA:BEFORE:MethodFour", 
                "AspectB:BEFORE:MethodFour", 
                "COMPONENT:MethodFour(456)", 
                "AspectB:AFTER:MethodFour",
                "AspectA:AFTER:MethodFour",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_ModifyInputOutput()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestInspectorAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);
            var aspectProxy = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectProxy.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABCABC");
            
            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "AspectC:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(246)", 
                "AspectC:AFTER:MethodTwo",
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_ExplicitInterfaceImplementations()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA", qualifiedMemberNames: true),
                new TestAspectConvention.AspectProvider("AspectB", qualifiedMemberNames: true)
            );
            var component = new TestComponentFour(_log);
            var aspectProxy = factoryUnderTest.CreateProxy(component, new[] {
                typeof(ITestComponent), typeof(ITestComponentTwo), typeof(ITestComponentFour)
            });

            //-- act

            ((ITestComponent)aspectProxy).MethodOne();
            var returnValueTwo = ((ITestComponentTwo)aspectProxy).MethodFour(num: 123);

            ((ITestComponentFour)aspectProxy).MethodOne();
            var returnValueFour = ((ITestComponentFour)aspectProxy).MethodFour(num: 456);

            //-- assert

            returnValueTwo.ShouldBe("DEF");
            returnValueFour.ShouldBe("XDEF");

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:ITestComponent.MethodOne", 
                "AspectB:BEFORE:ITestComponent.MethodOne", 
                "COMPONENT:MethodOne", 
                "AspectB:AFTER:ITestComponent.MethodOne",
                "AspectA:AFTER:ITestComponent.MethodOne",

                "AspectA:BEFORE:ITestComponentTwo.MethodFour", 
                "AspectB:BEFORE:ITestComponentTwo.MethodFour", 
                "COMPONENT:MethodFour(123)", 
                "AspectB:AFTER:ITestComponentTwo.MethodFour",
                "AspectA:AFTER:ITestComponentTwo.MethodFour",

                "AspectA:BEFORE:ITestComponentFour.MethodOne", 
                "AspectB:BEFORE:ITestComponentFour.MethodOne", 
                "COMPONENT:ITestComponentFour.MethodOne", 
                "AspectB:AFTER:ITestComponentFour.MethodOne",
                "AspectA:AFTER:ITestComponentFour.MethodOne",

                "AspectA:BEFORE:ITestComponentFour.MethodFour", 
                "AspectB:BEFORE:ITestComponentFour.MethodFour", 
                "COMPONENT:ITestComponentFour.MethodFour(456)", 
                "AspectB:AFTER:ITestComponentFour.MethodFour",
                "AspectA:AFTER:ITestComponentFour.MethodFour",

            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Factory_AspectInjection()
        {
            //-- arrange

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(base.DyamicModule).AsSelf();
            containerBuilder.RegisterInstance(_log).AsSelf();
            containerBuilder.RegisterInstance(Framework.Components.Resolve<PipelineObjectFactory>()).AsSelf();

            containerBuilder.RegisterType<ComponentAspectFactory>();
            containerBuilder.RegisterPipeline<IComponentAspectProvider>();
            containerBuilder.RegisterInstance(new TestAspectConvention.AspectProvider("AspectB")).As<IComponentAspectProvider>().LastInPipeline();
            containerBuilder.RegisterInstance(new TestAspectConvention.AspectProvider("AspectA")).As<IComponentAspectProvider>().FirstInPipeline();
            
            var container = containerBuilder.Build();
            var factoryUnderTest = container.Resolve<ComponentAspectFactory>();
                
            var component = new TestComponentOne(_log);
            var aspectWrapper = (ITestComponent)factoryUnderTest.CreateProxy(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectWrapper.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABC");

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Proxy_ResolveFromContainer()
        {
            //-- arrange

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(base.DyamicModule).AsSelf();
            containerBuilder.RegisterInstance(_log).AsSelf();
            containerBuilder.RegisterInstance(Framework.Components.Resolve<PipelineObjectFactory>()).AsSelf();

            containerBuilder.RegisterType<ComponentAspectFactory>();
            containerBuilder.RegisterPipeline<IComponentAspectProvider>();
            containerBuilder.RegisterInstance(new TestAspectConvention.AspectProvider("AspectB")).As<IComponentAspectProvider>().LastInPipeline();
            containerBuilder.RegisterInstance(new TestAspectConvention.AspectProvider("AspectA")).As<IComponentAspectProvider>().FirstInPipeline();

            containerBuilder.RegisterType<TestComponentTwo>().AsSelf();
            containerBuilder.Register(c => c.Resolve<ComponentAspectFactory>().CreateProxy(c.Resolve<TestComponentTwo>())).As<ITestComponent, ITestComponentTwo>().SingleInstance();

            var container = containerBuilder.Build();

            //-- act

            var one = container.Resolve<ITestComponent>();
            var two = container.Resolve<ITestComponentTwo>();
            
            var returnValue1 = one.MethodTwo(num: 123);
            var returnValue2 = two.MethodFour(num: 456);

            //-- assert

            returnValue1.ShouldBe("ABC");
            returnValue2.ShouldBe("DEF");

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",

                "AspectA:BEFORE:MethodFour", 
                "AspectB:BEFORE:MethodFour", 
                "COMPONENT:MethodFour(456)", 
                "AspectB:AFTER:MethodFour",
                "AspectA:AFTER:MethodFour",
            });

            two.ShouldBeSameAs(one);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_SingleAspect_Create()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));

            //-- act

            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- assert

            aspectized.ShouldNotBeNull();
            aspectized.ShouldBeAssignableTo<ITestComponent>();
            aspectized.ShouldBeAssignableTo<ITestComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_MultipleAspects_Create()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );

            //-- act

            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- assert

            aspectized.ShouldNotBeNull();
            aspectized.ShouldBeAssignableTo<ITestComponent>();
            aspectized.ShouldBeAssignableTo<ITestComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_SingleAspect_InvokeVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- act

            aspectized.MethodOne();

            //-- assert

            _log.ShouldBe(new[] {
                "Test:BEFORE:MethodOne", 
                "COMPONENT:MethodOne", 
                "Test:AFTER:MethodOne"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_MultipleAspects_InvokeVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- act

            aspectized.MethodOne();

            //-- assert

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodOne", 
                "AspectB:BEFORE:MethodOne", 
                "AspectC:BEFORE:MethodOne", 
                "COMPONENT:MethodOne", 
                "AspectC:AFTER:MethodOne",
                "AspectB:AFTER:MethodOne",
                "AspectA:AFTER:MethodOne",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_SingleAspect_InvokeFunction()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- act

            var returnValue = aspectized.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABC");
            
            _log.ShouldBe(new[] {
                "Test:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "Test:AFTER:MethodTwo"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_MultipleAspects_InvokeFunction()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- act

            var returnValue = aspectized.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABC");
            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "AspectC:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "AspectC:AFTER:MethodTwo",
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_ModifyInputOutput()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestInspectorAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- act

            var returnValue = aspectized.MethodTwo(num: 123);

            //-- assert

            returnValue.ShouldBe("ABCABC");

            _log.ShouldBe(new[] {
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "AspectC:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(246)", 
                "AspectC:AFTER:MethodTwo",
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_ResolveFromContainer()
        {
            //-- arrange

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(base.DyamicModule).AsSelf();
            containerBuilder.RegisterInstance(_log).AsSelf();
            containerBuilder.RegisterInstance(Framework.Components.Resolve<PipelineObjectFactory>()).AsSelf();

            containerBuilder.RegisterType<ComponentAspectFactory>();
            containerBuilder.RegisterPipeline<IComponentAspectProvider>();
            containerBuilder.RegisterInstance(new TestAspectConvention.AspectProvider("AspectB")).As<IComponentAspectProvider>().LastInPipeline();
            containerBuilder.RegisterInstance(new TestAspectConvention.AspectProvider("AspectA")).As<IComponentAspectProvider>().FirstInPipeline();

            containerBuilder.Register(c => c.Resolve<ComponentAspectFactory>().CreateInheritor(typeof(TestComponentThree)))
                .As<TestComponentThree, ITestComponent, ITestComponentTwo>()
                .SingleInstance();

            var container = containerBuilder.Build();

            //-- act

            var one = container.Resolve<ITestComponent>();
            var two = container.Resolve<ITestComponentTwo>();
            var three = container.Resolve<TestComponentThree>();

            _log.Add("-1-");
            var returnValue1 = one.MethodTwo(num: 123);
            
            _log.Add("-2-");
            var returnValue2 = two.MethodFour(num: 456);
            
            _log.Add("-3-");
            var returnValue3 = three.MethodTwo(num: 1123);

            _log.Add("-4-");
            var returnValue4 = three.MethodFour(num: 1456);

            //-- assert

            returnValue1.ShouldBe("ABC");
            returnValue2.ShouldBe("DEF");
            returnValue3.ShouldBe("ABC");
            returnValue4.ShouldBe("DEF");

            _log.ShouldBe(new[] {
                "-1-",
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(123)", 
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
                "-2-",
                "AspectA:BEFORE:MethodFour", 
                "AspectB:BEFORE:MethodFour", 
                "COMPONENT:MethodFour(456)", 
                "AspectB:AFTER:MethodFour",
                "AspectA:AFTER:MethodFour",
                "-3-",
                "AspectA:BEFORE:MethodTwo", 
                "AspectB:BEFORE:MethodTwo", 
                "COMPONENT:MethodTwo(1123)", 
                "AspectB:AFTER:MethodTwo",
                "AspectA:AFTER:MethodTwo",
                "-4-",
                "AspectA:BEFORE:MethodFour", 
                "AspectB:BEFORE:MethodFour", 
                "COMPONENT:MethodFour(1456)", 
                "AspectB:AFTER:MethodFour",
                "AspectA:AFTER:MethodFour",
            });

            two.ShouldBeSameAs(one);
            three.ShouldBeSameAs(one);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Inheritor_MembersOfSystemObjectAreNotOverridden()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var aspectized = (TestComponentThree)factoryUnderTest.CreateInheritor(typeof(TestComponentThree));

            //-- act

            var aspectizedType = aspectized.GetType();
            var toStringMethod = aspectizedType.GetMethod("ToString");

            //-- assert

            toStringMethod.DeclaringType.ShouldBe(typeof(object));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ComponentAspectFactory CreateFactoryUnderTest(params IComponentAspectProvider[] aspectProviders)
        {
            var aspectPipeline = new Pipeline<IComponentAspectProvider>(aspectProviders);
            var factoryUnderTest = new ComponentAspectFactory(Framework.Components, base.DyamicModule, aspectPipeline);
            return factoryUnderTest;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetShortMemberName(MemberBase member, bool qualified)
        {
            var name = member.Name;

            if (name.Contains('.'))
            {
                var nameParts = name.Split('.', '+');

                if (qualified)
                {
                    return string.Join(".", nameParts.Skip(nameParts.Length - 2));
                }
                else
                {
                    return nameParts.Last();
                }
            }

            return name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestAspectConvention : DecorationConvention
        {
            private readonly string _name;
            private readonly ComponentAspectFactory.ConventionContext _aspectContext;
            private readonly bool _qualifiedMemberNames;
            private Field<List<string>> _logField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestAspectConvention(string name, ComponentAspectFactory.ConventionContext aspectContext, bool qualifiedMemberNames = false)
                : base(Will.DecorateMethods)
            {
                _name = name;
                _aspectContext = aspectContext;
                _qualifiedMemberNames = qualifiedMemberNames;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DecorationConvention

            protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
            {
                _logField = _aspectContext.GetDependencyField<List<string>>(classWriter);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
            {
                decorate()
                    .OnBefore(w => _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(
                        _name + 
                        ":BEFORE:" + 
                        GetShortMemberName(member, _qualifiedMemberNames))))
                    .OnAfter(w => _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(
                        _name + 
                        ":AFTER:" + 
                        GetShortMemberName(member, _qualifiedMemberNames))));
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class AspectProvider : IComponentAspectProvider
            {
                private readonly string _name;
                private readonly bool _qualifiedMemberNames;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public AspectProvider(string name, bool qualifiedMemberNames = false)
                {
                    _name = name;
                    _qualifiedMemberNames = qualifiedMemberNames;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IComponentAspectProvider

                public IObjectFactoryConvention GetAspectConvention(ComponentAspectFactory.ConventionContext context)
                {
                    return new TestAspectConvention(_name, context, _qualifiedMemberNames);
                }

                #endregion
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestInspectorAspectConvention : DecorationConvention
        {
            private readonly string _name;
            private readonly ComponentAspectFactory.ConventionContext _aspectContext;
            private Field<List<string>> _logField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestInspectorAspectConvention(string name, ComponentAspectFactory.ConventionContext aspectContext)
                : base(Will.DecorateMethods)
            {
                _name = name;
                _aspectContext = aspectContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DecorationConvention

            protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
            {
                _logField = _aspectContext.GetDependencyField<List<string>>(classWriter);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
            {
                decorate()
                    .OnBefore(w => {
                        _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(
                            _name + 
                            ":BEFORE:" + 
                            GetShortMemberName(member, qualified: false)));
                        w.ForEachArgument((arg, argIndex) => {
                            if (arg.OperandType == typeof(int))
                            {
                                w.Argument<int>(argIndex + 1).Assign(w.Argument<int>(argIndex + 1) * w.Const(2));
                            }
                        });
                    })
                    .OnReturnValue((w, retVal) => {
                        if (retVal.OperandType == typeof(string))
                        {
                            var newRetVal = w.Local<string>();
                            newRetVal.Assign(retVal.CastTo<string>() + retVal.CastTo<string>());
                            retVal.Assign(newRetVal.CastTo<TT.TReturn>());
                        }                            
                    })
                    .OnAfter(w => 
                        _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(
                            _name + 
                            ":AFTER:" +
                            GetShortMemberName(member, qualified: false)))
                    );
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class AspectProvider : IComponentAspectProvider
            {
                private readonly string _name;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public AspectProvider(string name)
                {
                    _name = name;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Implementation of IComponentAspectProvider

                public IObjectFactoryConvention GetAspectConvention(ComponentAspectFactory.ConventionContext context)
                {
                    return new TestInspectorAspectConvention(_name, context);
                }

                #endregion
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestComponent
        {
            void MethodOne();
            string MethodTwo(int num);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestComponentTwo
        {
            void MethodThree();
            string MethodFour(int num);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestComponentFour
        {
            void MethodOne();
            string MethodFour(int num);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestComponentOne : ITestComponent
        {
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestComponentOne(List<string> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponent

            public void MethodOne()
            {
                _log.Add("COMPONENT:MethodOne");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string MethodTwo(int num)
            {
                _log.Add("COMPONENT:MethodTwo(" + num + ")");
                return "ABC";
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestComponentTwo : ITestComponent, ITestComponentTwo
        {
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestComponentTwo(List<string> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponent

            public void MethodOne()
            {
                _log.Add("COMPONENT:MethodOne");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string MethodTwo(int num)
            {
                _log.Add("COMPONENT:MethodTwo(" + num + ")");
                return "ABC";
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponentTwo
            
            void ITestComponentTwo.MethodThree()
            {
                _log.Add("COMPONENT:MethodThree");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            string ITestComponentTwo.MethodFour(int num)
            {
                _log.Add("COMPONENT:MethodFour(" + num + ")");
                return "DEF";
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestComponentThree : ITestComponent, ITestComponentTwo
        {
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestComponentThree(List<string> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponent

            public virtual void MethodOne()
            {
                _log.Add("COMPONENT:MethodOne");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string MethodTwo(int num)
            {
                _log.Add("COMPONENT:MethodTwo(" + num + ")");
                return "ABC";
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponentTwo

            public virtual void MethodThree()
            {
                _log.Add("COMPONENT:MethodThree");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string MethodFour(int num)
            {
                _log.Add("COMPONENT:MethodFour(" + num + ")");
                return "DEF";
            }

            #endregion
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestComponentFour : ITestComponent, ITestComponentTwo, ITestComponentFour
        {
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestComponentFour(List<string> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponent

            public virtual void MethodOne()
            {
                _log.Add("COMPONENT:MethodOne");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string MethodTwo(int num)
            {
                _log.Add("COMPONENT:MethodTwo(" + num + ")");
                return "ABC";
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponentTwo

            public virtual void MethodThree()
            {
                _log.Add("COMPONENT:MethodThree");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string MethodFour(int num)
            {
                _log.Add("COMPONENT:MethodFour(" + num + ")");
                return "DEF";
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestComponenFour

            void ITestComponentFour.MethodOne()
            {
                _log.Add("COMPONENT:ITestComponentFour.MethodOne");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            string ITestComponentFour.MethodFour(int num)
            {
                _log.Add("COMPONENT:ITestComponentFour.MethodFour(" + num + ")");
                return "XDEF";
            }

            #endregion
        }
    }
}

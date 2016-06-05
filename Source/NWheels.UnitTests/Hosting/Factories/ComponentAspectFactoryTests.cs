using System;
using System.Collections.Generic;
using System.Linq;
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
        public void CanAspectizeComponent()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var component = new TestComponentOne(_log);

            //-- act

            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- assert

            aspectizedWrapper.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAspectizeComponentWithMultipleAspects()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);

            //-- act

            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- assert

            aspectizedWrapper.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAspectizeComponentWithMultipleInterfaces()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentTwo(_log);

            //-- act

            var aspectizedWrapper = factoryUnderTest.Aspectize(component, new[] {
                typeof(ITestComponent), 
                typeof(ITestComponentTwo)
            });

            //-- assert

            aspectizedWrapper.ShouldNotBeNull();
            aspectizedWrapper.ShouldBeAssignableTo<ITestComponent>();
            aspectizedWrapper.ShouldBeAssignableTo<ITestComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInvokeVoidMethodOnAspectWrapper()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var component = new TestComponentOne(_log);
            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- act

            aspectizedWrapper.MethodOne();

            //-- assert

            _log.ShouldBe(new[] {
                "Test:BEFORE:MethodOne", 
                "COMPONENT:MethodOne", 
                "Test:AFTER:MethodOne"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInvokeVoidMethodOnAspectWrapperWithMultipleAspects()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);
            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- act

            aspectizedWrapper.MethodOne();

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
        public void CanInvokeVoidMethodOnAspectWrapperWithMultipleInterfaces()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB")
            );
            
            var component = new TestComponentTwo(_log);
            
            var aspectWrapper = factoryUnderTest.Aspectize(component, new[] {
                typeof(ITestComponent), 
                typeof(ITestComponentTwo)
            });

            //-- act

            ((ITestComponent)aspectWrapper).MethodOne();
            ((ITestComponentTwo)aspectWrapper).MethodThree();

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
        public void CanInvokeFunctionOnAspectWrapper()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(new TestAspectConvention.AspectProvider("Test"));
            var component = new TestComponentOne(_log);
            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectizedWrapper.MethodTwo(num: 123);

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
        public void CanInvokeFunctionOnAspectWrapperWithMultipleAspects()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);
            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectizedWrapper.MethodTwo(num: 123);

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
        public void CanInvokeFunctionOnAspectWrapperWithMultipleInterfaces()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestAspectConvention.AspectProvider("AspectB")
            );
            
            var component = new TestComponentTwo(_log);
            
            var aspectWrapper = factoryUnderTest.Aspectize(component, new[] {
                typeof(ITestComponent), 
                typeof(ITestComponentTwo)
            });

            //-- act

            var returnValue1 = ((ITestComponent)aspectWrapper).MethodTwo(num: 123);
            var returnValue2 = ((ITestComponentTwo)aspectWrapper).MethodFour(num: 456);

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
        public void CanModifyMethodInputAndOutputInApsect()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest(
                new TestAspectConvention.AspectProvider("AspectA"),
                new TestInspectorAspectConvention.AspectProvider("AspectB"),
                new TestAspectConvention.AspectProvider("AspectC")
            );
            var component = new TestComponentOne(_log);
            var aspectizedWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

            //-- act

            var returnValue = aspectizedWrapper.MethodTwo(num: 123);

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
        public void CanRegisterAspectsInIocContainer()
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
            var aspectWrapper = (ITestComponent)factoryUnderTest.Aspectize(component, new[] { typeof(ITestComponent) });

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
        public void CanRegisterAspectizedComponentInIocContainer()
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
            containerBuilder.Register(c => c.Resolve<ComponentAspectFactory>().Aspectize(c.Resolve<TestComponentTwo>())).As<ITestComponent, ITestComponentTwo>().SingleInstance();

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

        private ComponentAspectFactory CreateFactoryUnderTest(params IComponentAspectProvider[] aspectProviders)
        {
            var aspectPipeline = new Pipeline<IComponentAspectProvider>(aspectProviders);
            var factoryUnderTest = new ComponentAspectFactory(Framework.Components, base.DyamicModule, aspectPipeline);
            return factoryUnderTest;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestAspectConvention : DecorationConvention
        {
            private readonly string _name;
            private readonly ComponentAspectFactory.ConventionContext _aspectContext;
            private Field<List<string>> _logField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestAspectConvention(string name, ComponentAspectFactory.ConventionContext aspectContext)
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
                    .OnBefore(w => _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(_name + ":BEFORE:" + member.Name)))
                    .OnAfter(w => _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(_name + ":AFTER:" + member.Name)));
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
                    return new TestAspectConvention(_name, context);
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
                        _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(_name + ":BEFORE:" + member.Name));
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
                        _logField.Add(_aspectContext.StaticStrings.GetStaticStringOperand(_name + ":AFTER:" + member.Name))
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
    }
}

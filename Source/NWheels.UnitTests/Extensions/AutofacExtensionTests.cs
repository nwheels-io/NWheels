using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Metadata;
using NUnit.Framework;
using NWheels.Extensions;

namespace NWheels.UnitTests.Extensions
{
    [TestFixture]
    public class AutofacExtensionTests
    {
        [Test]
        public void RegisterPipeline_Resolve_OrderedAsExpected()
        {
            //-- arrange

            var builder = new ContainerBuilder();

            builder.RegisterPipeline<ITestComponent>();
            builder.RegisterType<ComponentC>().As<ITestComponent>();
            builder.RegisterType<ComponentB>().As<ITestComponent>().FirstInPipeline();
            builder.RegisterType<ComponentD>().As<ITestComponent>().LastInPipeline();
            builder.RegisterType<ComponentA>().As<ITestComponent>().FirstInPipeline();
            builder.RegisterType<ComponentE>().As<ITestComponent>().LastInPipeline();

            var container = builder.Build();

            //-- act

            var pipeline = container.ResolvePipeline<ITestComponent>();

            //-- assert

            Assert.That(pipeline.Count, Is.EqualTo(5));
            Assert.That(pipeline[0], Is.InstanceOf<ComponentA>());
            Assert.That(pipeline[1], Is.InstanceOf<ComponentB>());
            Assert.That(pipeline[2], Is.InstanceOf<ComponentC>());
            Assert.That(pipeline[3], Is.InstanceOf<ComponentD>());
            Assert.That(pipeline[4], Is.InstanceOf<ComponentE>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RegisterPipeline_SingleInstance_PipelineInitializedOnce()
        {
            //-- arrange

            var builder = new ContainerBuilder();

            builder.RegisterPipeline<ITestComponent>().SingleInstance();
            builder.RegisterType<ComponentB>().As<ITestComponent>();
            builder.RegisterType<ComponentC>().As<ITestComponent>().LastInPipeline();
            builder.RegisterType<ComponentA>().As<ITestComponent>().FirstInPipeline();

            var container = builder.Build();

            //-- act

            var pipeline1 = container.Resolve<Pipeline<ITestComponent>>();
            var pipeline2 = container.Resolve<Pipeline<ITestComponent>>();

            //-- assert

            Assert.That(pipeline2, Is.SameAs(pipeline1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RegisterPipeline_InstancePerDependency_NewPipelineInitializedEachTime()
        {
            //-- arrange

            var builder = new ContainerBuilder();

            builder.RegisterPipeline<ITestComponent>().InstancePerDependency();
            builder.RegisterType<ComponentB>().As<ITestComponent>();
            builder.RegisterType<ComponentC>().As<ITestComponent>().LastInPipeline();
            builder.RegisterType<ComponentA>().As<ITestComponent>().FirstInPipeline();

            var container = builder.Build();

            //-- act

            var pipeline1 = container.Resolve<Pipeline<ITestComponent>>();
            var pipeline2 = container.Resolve<Pipeline<ITestComponent>>();

            //-- assert

            Assert.That(pipeline2, Is.Not.SameAs(pipeline1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RegisterPipeline_SingleInstance_SameInstancesReturned()
        {
            //-- arrange

            var builder = new ContainerBuilder();

            builder.RegisterPipeline<ITestComponent>().SingleInstance();
            builder.RegisterType<ComponentB>().As<ITestComponent>();
            builder.RegisterType<ComponentC>().As<ITestComponent>().LastInPipeline();
            builder.RegisterType<ComponentA>().As<ITestComponent>().FirstInPipeline();

            var container = builder.Build();

            //-- act

            var pipeline1 = container.ResolvePipeline<ITestComponent>();
            var pipeline2 = container.ResolvePipeline<ITestComponent>();

            //-- assert

            Assert.That(pipeline1.Count, Is.EqualTo(3));
            Assert.That(pipeline1[0], Is.InstanceOf<ComponentA>());
            Assert.That(pipeline1[1], Is.InstanceOf<ComponentB>());
            Assert.That(pipeline1[2], Is.InstanceOf<ComponentC>());
            
            Assert.That(pipeline2.Count, Is.EqualTo(pipeline1.Count));
            Assert.That(pipeline2[0], Is.SameAs(pipeline1[0]));
            Assert.That(pipeline2[1], Is.SameAs(pipeline1[1]));
            Assert.That(pipeline2[2], Is.SameAs(pipeline1[2]));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RegisterPipeline_InstancePerDependency_NewInstancesReturnedEachTime()
        {
            //-- arrange

            var builder = new ContainerBuilder();

            builder.RegisterPipeline<ITestComponent>().InstancePerDependency();
            builder.RegisterType<ComponentB>().As<ITestComponent>().InstancePerDependency();
            builder.RegisterType<ComponentC>().As<ITestComponent>().LastInPipeline().InstancePerDependency();
            builder.RegisterType<ComponentA>().As<ITestComponent>().FirstInPipeline().InstancePerDependency();

            var container = builder.Build();

            //-- act

            var pipeline1 = container.ResolvePipeline<ITestComponent>();
            var pipeline2 = container.ResolvePipeline<ITestComponent>();

            //-- assert

            Assert.That(pipeline1.Count, Is.EqualTo(3));
            Assert.That(pipeline1[0], Is.InstanceOf<ComponentA>());
            Assert.That(pipeline1[1], Is.InstanceOf<ComponentB>());
            Assert.That(pipeline1[2], Is.InstanceOf<ComponentC>());

            Assert.That(pipeline2.Count, Is.EqualTo(pipeline1.Count));
            Assert.That(pipeline2[0], Is.Not.SameAs(pipeline1[0]));
            Assert.That(pipeline2[1], Is.Not.SameAs(pipeline1[1]));
            Assert.That(pipeline2[2], Is.Not.SameAs(pipeline1[2]));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestComponent
        {
        }
        public class ComponentA : ITestComponent
        {
        }
        public class ComponentB : ITestComponent
        {
        }
        public class ComponentC : ITestComponent
        {
        }
        public class ComponentD : ITestComponent
        {
        }
        public class ComponentE : ITestComponent
        {
        }
    }
}

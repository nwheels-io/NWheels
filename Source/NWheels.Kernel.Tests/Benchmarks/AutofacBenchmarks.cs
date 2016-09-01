using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Xunit;
using Xunit.Abstractions;

namespace NWheels.Kernel.Tests.Benchmarks
{
    public class AutofacBenchmarks
    {
        private readonly ITestOutputHelper _output;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutofacBenchmarks(ITestOutputHelper output)
        {
            _output = output;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveSingleton()
        {
            var outerRunCount = 10;
            var innerCycleCount = 1000;

            var container = BuildAutofacContainer();
            var times = new List<TimeSpan>(capacity: outerRunCount * innerCycleCount);
            var clock = Stopwatch.StartNew();

            for (int i = 0; i < outerRunCount; i++)
            {
                for (int j = 0; j < innerCycleCount; j++)
                {
                    clock.Restart();
                    container.Resolve<ITestSingletonComponent>();
                    times.Add(clock.Elapsed);
                }
            }

            var totalTicks = times.Skip(outerRunCount * innerCycleCount / 2).Select(t => t.Ticks).Sum();
            var totalTime = TimeSpan.FromTicks((long)totalTicks);

            var averageTicks = times.Skip(outerRunCount * innerCycleCount / 2).Select(t => t.Ticks).Average();
            var averageTime = TimeSpan.FromTicks((long)averageTicks);

            var location = this.GetType().GetTypeInfo().Assembly.Location;
            File.AppendAllLines(
                Path.Combine(Path.GetDirectoryName(location), "AutofacBenchmarks.test.log"),
                new[] {
                    $"AUTOFAC - SINGLETONE - cycles: {outerRunCount * innerCycleCount / 2}, total time: {totalTime}, average time: {averageTime}"
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IComponentContext BuildAutofacContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TestSingletonComponent>().As<ITestSingletonComponent>().SingleInstance();
            builder.RegisterType<TestTransientComponent>().As<ITestTransientComponent>().InstancePerDependency();

            return builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestSingletonComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestSingletonComponent : ITestSingletonComponent
        {

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestTransientComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestTransientComponent : ITestTransientComponent
        {

        }
    }
}
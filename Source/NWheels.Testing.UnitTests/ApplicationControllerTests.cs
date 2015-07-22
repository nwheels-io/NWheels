using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Testing.Controllers;

namespace NWheels.Testing.UnitTests
{
    [TestFixture]
    public class ApplicationControllerTests : SystemTestBase
    {
        private Stopwatch _clock;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _clock = Stopwatch.StartNew();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanStartAndStopSingleNodeInstanceApplication()
        {
            //-- arrange

            var controller = new ApplicationController(
                new ConsolePlainLog("CONTROLLER", LogLevel.Debug, _clock), 
                CreateBootConfig(),
                onInjectComponents: InjectHostComponents);

            //-- act

            controller.Load();

            if ( controller.CurrentState == NodeState.Standby )
            {
                controller.Activate();

                if ( controller.CurrentState == NodeState.Active )
                {
                    Console.WriteLine("HOORAY!!!! WE'RE UP AND RUNNING!");
                    controller.Deactivate();
                }
                else
                {
                    Console.WriteLine("FAILED TO ACTIVATE!!!");
                }

                controller.Unload();
            }
            else
            {
                Console.WriteLine("FAILED TO LOAD!!!");
            }

            //-- assert
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InjectHostComponents(Autofac.ContainerBuilder builder)
        {
            builder.RegisterInstance(new ConsolePlainLog("APP-NODE", LogLevel.Debug, _clock)).As<IPlainLog>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private BootConfiguration CreateBootConfig()
        {
            var configuration = new BootConfiguration {
                ApplicationName = "TEST",
                EnvironmentName = "TEST",
                EnvironmentType = "DEV",
                NodeName = "TEST",
                InstanceId = "1",
                FrameworkModules = new List<BootConfiguration.ModuleConfig>(),
                ApplicationModules = new List<BootConfiguration.ModuleConfig> { 
                    //new BootConfiguration.ModuleConfig {
                    //    Assembly = typeof(SingleNodeSystemTestBase.TestModule).Assembly.GetName().Name + ".dll",
                    //    LoaderClass = typeof(SingleNodeSystemTestBase.TestModule).FullName
                    //}
                },
                ConfigFiles = new List<BootConfiguration.ConfigFile>(),
                LoadedFromDirectory = TestContext.CurrentContext.TestDirectory
            };

            configuration.Validate();
            return configuration;
        }
    }
}

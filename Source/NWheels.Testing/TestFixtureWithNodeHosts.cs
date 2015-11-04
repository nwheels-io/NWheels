using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Testing.Controllers;
using NWheels.Testing.Entities.Impl;

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class TestFixtureWithNodeHosts : TestFixtureBase
    {
        private ApplicationController _controller;
        private Stopwatch _clock;
        private ITestFixtureBaseLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public virtual void BaseSetUp()
        {
            if ( ShouldAutoStartApplication )
            {
                Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                StartApplication();
                _logger = AgentComponent.Components.Resolve<ITestFixtureBaseLogger>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TearDown]
        public virtual void BaseTearDown()
        {
            if ( ShouldAutoStartApplication )
            {
                StopApplication();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestAgentComponent AgentComponent { get; internal set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnBuildingBootConfig(BootConfiguration configuration)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnRegisteringHostComponents(Autofac.ContainerBuilder builder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual void OnRegisteringModuleComponents(Autofac.ContainerBuilder builder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual void OnInitializingAgentComponent()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void StartApplication()
        {
            var bootConfig = CreateBootConfiguration();

            _clock = Stopwatch.StartNew();
            _controller = new ApplicationController(new ConsolePlainLog(" controller ", LogLevel.Debug, _clock), bootConfig);
            
            _controller.InjectingComponents += (sender, args) => {
                args.ContainerBuilder.RegisterInstance<TestFixtureWithNodeHosts>(this).AsSelf().As<TestFixtureWithNodeHosts>();
                args.ContainerBuilder
                    .RegisterInstance(new ConsolePlainLog(string.Format(" {0}[{1}] ", NodeName, NodeInstanceId), LogLevel.Debug, _clock))
                    .As<IPlainLog>();

                if ( !HasDatabase )
                {
                    args.ContainerBuilder.RegisterType<VoidStorageInitializer>().As<IStorageInitializer>();
                }

                OnRegisteringHostComponents(args.ContainerBuilder);
            };

            _controller.LoadAndActivate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void StopApplication()
        {
            _controller.DeactivateAndUnload();
            _controller = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TestFixtureBase.ITestFixtureBaseLogger Logger
        {
            get { return _logger; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string ApplicationName
        {
            get { return "TESTAPP"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NodeName
        {
            get { return "TESTNODE"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string NodeInstanceId
        {
            get { return "1"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual bool HasDatabase
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual bool ShouldRebuildDatabase
        {
            get { return HasDatabase; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual bool ShouldAutoStartApplication
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BootConfiguration CreateBootConfiguration()
        {
            NWheels.Stacks.Nlog.NLogBasedPlainLog.Instance.ConfigureConsoleOutput();

            var bootConfig = new BootConfiguration {
                ApplicationName = this.ApplicationName,
                EnvironmentName = "TEST",
                EnvironmentType = "TEST",
                NodeName = this.NodeName,
                InstanceId = this.NodeInstanceId,
                FrameworkModules = new List<BootConfiguration.ModuleConfig>(),
                ApplicationModules = new List<BootConfiguration.ModuleConfig>(),
                ConfigFiles = new List<BootConfiguration.ConfigFile>(),
                LoadedFromDirectory = TestContext.CurrentContext.TestDirectory
            };

            TestAgentComponent.RegisterInBootConfig(bootConfig);
            OnBuildingBootConfig(bootConfig);

            bootConfig.Validate();
            return bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConsolePlainLog : IPlainLog
        {
            private readonly string _loggerName;
            private readonly LogLevel _logLevel;
            private readonly Stopwatch _clock;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ConsolePlainLog(string loggerName, LogLevel logLevel, Stopwatch clock)
            {
                _loggerName = loggerName;
                _logLevel = logLevel;
                _clock = clock;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reconfigure(IFrameworkLoggingConfiguration configuration)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void ConfigureConsoleOutput()
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void ConfigureWindowsEventLogOutput(string logName, string sourceName)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void LogNode(NWheels.Logging.LogNode node)
            {
                switch ( node.Level )
                {
                    case NWheels.Logging.LogLevel.Debug:
                    case NWheels.Logging.LogLevel.Verbose:
                        Debug(GetMessageIdQualifier(node.MessageId) + node.SingleLineText);
                        break;
                    case NWheels.Logging.LogLevel.Info:
                        Info(GetMessageIdQualifier(node.MessageId) + node.SingleLineText);
                        break;
                    case NWheels.Logging.LogLevel.Warning:
                        Warning(GetMessageIdQualifier(node.MessageId) + node.SingleLineText + AddExceptionIf(node.Exception));
                        break;
                    case NWheels.Logging.LogLevel.Error:
                        Error(GetMessageIdQualifier(node.MessageId) + node.SingleLineText + AddExceptionIf(node.Exception));
                        break;
                    case NWheels.Logging.LogLevel.Critical:
                        Critical(GetMessageIdQualifier(node.MessageId) + node.SingleLineText + AddExceptionIf(node.Exception));
                        break;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void LogActivity(NWheels.Logging.ActivityLogNode activity)
            {
                if ( activity.Parent != null )
                {
                    Trace(GetMessageIdQualifier(activity.MessageId) + activity.SingleLineText);
                }
                else
                {
                    Trace("[THREAD:{0}] {1}", activity.TaskType, GetMessageIdQualifier(activity.MessageId) + activity.SingleLineText);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Debug(string format, params object[] args)
            {
                if ( _logLevel == LogLevel.Debug )
                {
                    Console.WriteLine(GetLogPrefix() + ">debug> " + format.FormatIf(args));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Trace(string format, params object[] args)
            {
                if (_logLevel <= LogLevel.Verbose)
                {
                    Console.WriteLine(GetLogPrefix() + ">trace> " + format.FormatIf(args));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Info(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Info )
                {
                    Console.WriteLine(GetLogPrefix() + ">INFO > " + format.FormatIf(args));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Warning(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Warning )
                {
                    Console.WriteLine(GetLogPrefix() + ">WARNI> " + format.FormatIf(args));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Error(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Error )
                {
                    Console.WriteLine(GetLogPrefix() + ">ERROR> " + format.FormatIf(args));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Critical(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Critical )
                {
                    Console.WriteLine(GetLogPrefix() + ">CRITI> " + format.FormatIf(args));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private string GetLogPrefix()
            {
                return string.Format("+{0} >{1}", _clock.Elapsed, _loggerName);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private string GetMessageIdQualifier(string messageId)
            {
                var qualifiedLength = messageId.LastIndexOf('.');

                if ( qualifiedLength >= 0 )
                {
                    return messageId.Substring(0, qualifiedLength) + "::";
                }
                else
                {
                    return string.Empty;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private string AddExceptionIf(Exception exception)
            {
                if ( exception != null )
                {
                    return " + EXCEPTION[" + exception.GetType().Name + "]: " + exception.GetMessageDeep();
                }
                else
                {
                    return "";
                }
            }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class VoidStorageInitializer : IStorageInitializer
        {
            #region Implementation of IStorageInitializer

            public bool StorageSchemaExists(string connectionString)
            {
                throw new NotSupportedException();
            }

            public void MigrateStorageSchema(string connectionString, DataRepositoryBase context, SchemaMigrationCollection migrations)
            {
                throw new NotSupportedException();
            }

            public void CreateStorageSchema(string connectionString)
            {
                throw new NotSupportedException();
            }

            public void DropStorageSchema(string connectionString)
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}

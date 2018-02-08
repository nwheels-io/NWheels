using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;
using NWheels.Kernel.Api.Logging;
using NWheels.Microservices.Api;

namespace NWheels.Microservices.Runtime.Cli
{
    public class InspectCommand : CliCommandBase
    {
        private readonly MicroserviceHost _host;
        private readonly MutableBootConfiguration _bootConfig;
        private bool _debugLogLevel;
        private bool _verboseLogLevel;
        private bool _inspectBootConfig;
        private bool _inspectConfigSections;
        private bool _inspectLifecycleComponents;
        private bool _inspectComponentContainer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InspectCommand(MicroserviceHost host, MutableBootConfiguration bootConfig) 
            : base("inspect", "Inspect modules, features, components, and configuration")
        {
            _host = host;
            _bootConfig = bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("d|debug", ref _debugLogLevel, "Set log level to Debug");
            syntax.DefineOption("v|verbose", ref _verboseLogLevel, "Set log level to Verbose");
            syntax.DefineOption("bc|boot-config", ref _inspectBootConfig, "Inspect list of modules, features, and host components");
            syntax.DefineOption("cs|config-sections", ref _inspectConfigSections, "Inspect values in configuration sections");
            syntax.DefineOption("lc|lifecycle-components", ref _inspectLifecycleComponents, "Inspect list of lifecycle components");
            syntax.DefineOption("cc|component-container", ref _inspectComponentContainer, "Inspect registrations in DI container");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            var anyOptionSpecified = (
                _debugLogLevel |
                _verboseLogLevel |
                _inspectBootConfig |
                _inspectConfigSections |
                _inspectLifecycleComponents |
                _inspectComponentContainer);

            if (!anyOptionSpecified)
            {
                arguments.ReportError("At least one of -d, -v, -bc, -cs, -lc, or -cc options must be specified.");
            }

            if (_debugLogLevel)
            {
                _bootConfig.LogLevel = LogLevel.Debug;
            }
            else if (_verboseLogLevel)
            {
                _bootConfig.LogLevel = LogLevel.Verbose;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Execute(CancellationToken cancellation)
        {
            try
            {
                _host.Configure(cancellation);
            }
            catch (Exception error)
            {
                throw new CliCommandFailedException(error, exitCode: -1);
            }

            try
            {
                DoInspections();
            }
            catch (Exception error)
            {
                throw new CliCommandFailedException(error, exitCode: -2);
            }

            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DoInspections()
        {
            if (_inspectBootConfig)
            {
                DoInspectBootConfig();
            }

            if (_inspectConfigSections)
            {
                DoInspectConfigSections();
            }

            if (_inspectLifecycleComponents)
            {
                DoInspectLifecycleComponents();
            }

            if (_inspectComponentContainer)
            {
                DoInspectComponentContainer();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DoInspectBootConfig()
        {
            //throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DoInspectConfigSections()
        {
            //throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DoInspectLifecycleComponents()
        {
            //throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DoInspectComponentContainer()
        {
            //throw new NotImplementedException();
        }
    }
}

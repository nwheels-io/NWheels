using Microsoft.Win32;
using NWheels.Hosting;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using Topshelf;
using Topshelf.HostConfigurators;
using Topshelf.Runtime;

namespace NWheels.Hosts.Console
{
    static class Program
    {
        private static bool _s_isAfterInstallCalled;
        private static string _s_instanceName;
        private static string _s_serviceName;
        private static ProgramConfig _s_programConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        const string BootFileParamKey = "bootfile";
        const string BatchjobParamKey = "batchjob";
        const string ConfigParamKey = "config";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Main()
        {
            CrashLog.RegisterUnhandledExceptionHandler();
            _s_programConfig = new ProgramConfig();
            
            HostFactory.Run(x => {
                x.UseNLog();
                x.Service(s => {
                    _s_programConfig.HostSettings = s;
                    return new NodeHostControl(_s_programConfig);
                });

                x.AddCommandLineDefinition(BootFileParamKey, v => {
                    if ( _s_programConfig.BootConfig == null )
                    {
                        _s_programConfig.BootConfigFilePath = v;
                    }
                });
                x.AddCommandLineDefinition(BatchjobParamKey, v => {
                    _s_programConfig.IsBatchJob = true;
                });
                x.AddCommandLineDefinition(ConfigParamKey, v => {
                    _s_programConfig.CommandLineConfigValues.Add(v);
                });
                x.ApplyCommandLine();

                if ( string.IsNullOrEmpty(_s_programConfig.BootConfigFilePath) )
                {
                    _s_programConfig.BootConfigFilePath = BootConfiguration.DefaultBootConfigFileName;
                }

                _s_programConfig.BootConfig = LoadBootConfig(_s_programConfig.BootConfigFilePath, hostConfig: x);
                
                x.RunAsLocalSystem();
                //x.BeforeInstall(BeforeInstall);
                x.AfterInstall(AfterInstall);
                
                //x.BeforeUninstall(BeforeUninstall);
                //x.AfterUninstall(AfterUninstall);

                //x.SetDescription("Sample Topshelf Host"); // Optional - deaults to service name
                //x.SetDisplayName("Sample Topshelf Host");  // Sets the Console window header (if exists); Display name in the services list. Optional, defaults to service name.
                //x.SetServiceName("SampleTopshelfHost");  // Unique per instance - Can be sent by InstanceName command-line option
            });

            // Fix commandline of service
            if ( _s_isAfterInstallCalled )
            {
                FixServiceCommandLineAfterInstall();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FixServiceCommandLineAfterInstall()
        {
            //System.Console.WriteLine("Fixed imagePath called ------");
            //System.Console.WriteLine("_s_serviceName = " + _s_serviceName);
            //System.Console.WriteLine("_s_instanceName = " + _s_instanceName);
            //System.Console.ReadLine();

            using ( var system = Registry.LocalMachine.OpenSubKey("SYSTEM") )
            using ( var controlSet = system.OpenSubKey("CurrentControlSet") )
            using ( var services = controlSet.OpenSubKey("services") )
            using ( var service = services.OpenSubKey(_s_serviceName, true) )
            {
                if ( service == null )
                    return;

                var imagePath = service.GetValue("ImagePath") as string;

                if ( string.IsNullOrEmpty(imagePath) )
                    return;

                string appendix = string.Format(" -{0} \"{1}\"", BootFileParamKey, _s_programConfig.BootConfigFilePath);
                if ( _s_programConfig.IsBatchJob )
                {
                    appendix += string.Format(" -{0}",BatchjobParamKey);
                    
                }
                foreach ( string v in _s_programConfig.CommandLineConfigValues )
                {
                    appendix += string.Format(" -{0} \"{1}\"", ConfigParamKey, v);
                }
                imagePath = imagePath + appendix;

                service.SetValue("ImagePath", imagePath);
                //System.Console.WriteLine("Fixed imagePath ------");
                //System.Console.ReadLine();
            }

        }

        //private static void BeforeUninstall()
        //{
        //    System.Console.WriteLine("Before Uninstall Called ------");
        //    System.Console.ReadLine();
        //}

        //private static void AfterUninstall()
        //{

        //    System.Console.WriteLine("After Uninstall Called ------");
        //    System.Console.ReadLine();
        //}

        //private static void BeforeInstall()
        //{
        //    System.Console.WriteLine("Before Install Called ------");
        //    System.Console.ReadLine();
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AfterInstall(InstallHostSettings s)
        {
            _s_instanceName = s.InstanceName;
            _s_serviceName = s.ServiceName;
            _s_isAfterInstallCalled = true;
            //System.Console.WriteLine("After Install Called ------");
            //System.Console.ReadLine();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static BootConfiguration LoadBootConfig(string filePath, HostConfigurator hostConfig)
        {
            IPlainLog log = NLogBasedPlainLog.Instance;

            log.Debug("Loading configuration from: {0}", filePath);

            //load boot config from path
            BootConfiguration bootConfig = BootConfiguration.LoadFromFile(filePath);
            bootConfig.Validate();

            log.Debug("> Application Name   - {0}", bootConfig.ApplicationName);
            log.Debug("> Node Name          - {0}", bootConfig.NodeName);

            foreach ( var module in bootConfig.FrameworkModules )
            {
                log.Debug("> Framework Module   - {0}", module.Name);
            }

            foreach ( var module in bootConfig.ApplicationModules )
            {
                log.Debug("> Application Module - {0}", module.Name);
            }

            //hostConfig.SetDescription("Late Sample Topshelf Host"); // Optional - deaults to service name
            //hostConfig.SetDisplayName("Late Sample Topshelf Host");  // Sets the Console window header (if exists); Display name in the services list. Optional, defaults to service name.
            hostConfig.SetServiceName(string.Format("{0}-{1}", bootConfig.ApplicationName, bootConfig.NodeName));
            //hostConfig.SetDisplayName(string.Format("{0} / {1} Host Process", bootConfig.ApplicationName, bootConfig.NodeName))
            //hostConfig.SetDisplayName(string.Format("Host Process for Application '{0}' {1}", bootConfig.ApplicationName, bootConfig.NodeName))

            return bootConfig;
        }
    }
}

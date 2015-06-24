using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using CommandLine;
using CommandLine.Text;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using NWheels.Tools.DevFlow;
using NWheels.Utilities.Core;

namespace NWheels.Tools.Cmd
{
    class Program
    {
        private static string[] _s_args;
        private static HelpText _s_help;
        private static IPlainLog _s_log;
        private static IContainer _s_container;
        private static string _s_toolName;
        private static object _s_toolOptions;
        private static UtilityToolBase _s_tool;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            _s_args = args;
            _s_help = null;

            _s_log = NLogBasedPlainLog.Instance;
            _s_log.ConfigureConsoleOutput();

            BuildContainer();

            var options = new MainOptions();
            var parser = new CommandLine.Parser();
            
            if ( !parser.ParseArguments(args, options, OnVerbCommand) )
            {
                _s_log.Error("Bad command line.");
                
                if ( _s_help == null )
                {
                    _s_help = HelpText.AutoBuild(options, current => HelpText.DefaultParsingErrorsHandler(options, current));
                }
                
                _s_log.Info("HELP\r\n\r\n{0}", _s_help);
                return 1;
            }

            _s_log.Info("Executing tool '{0}'.", _s_toolName);

            try
            {
                _s_tool = _s_container.ResolveNamed<UtilityToolBase>(_s_toolName);
            }
            catch ( Exception e )
            {
                _s_log.Error("Could not find or initialize tool '{0}'", _s_toolName);
                _s_log.Error(e.ToString());
                return 2;
            }

            try
            {
                _s_tool.ExecuteWithOptionsObject(_s_toolOptions);
            }
            catch ( Exception e )
            {
                _s_log.Error("Tool execution failed", _s_toolName);
                _s_log.Error(e.ToString());
                return 3;
            }

            _s_log.Info("ALL DONE!");
            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void OnVerbCommand(string verb, object verbOptions)
        {
            _s_toolName = verb;
            _s_toolOptions = verbOptions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(_s_log).As<IPlainLog>();
            builder.RegisterType<MergeSolutionTool>().Named<UtilityToolBase>(MergeSolutionTool.ToolName).InstancePerDependency();

            _s_container = builder.Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class MainOptions
        {
            public MainOptions()
            {
                MergeSolutionToolOptions = new MergeSolutionTool.Options();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [VerbOption(MergeSolutionTool.ToolName, HelpText = "Merge/unmerge projects of one solution into another solution")]
            public MergeSolutionTool.Options MergeSolutionToolOptions { get; set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [ParserState]
            public IParserState LastParserState { get; set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [HelpVerbOption]
            public string GetUsage(string verb)
            {
                var effectiveVerb = verb ?? (_s_args.Length >= 2 ? _s_args[1] : null);
                _s_help = HelpText.AutoBuild(this, effectiveVerb);
                
                _s_help.AddPreOptionsLine("\r\nUsage:");

                if ( effectiveVerb == null )
                {
                    _s_help.AddPreOptionsLine("\r\n  ntool.exe <verb> [options...]");
                    _s_help.AddPreOptionsLine("  ntool.exe help <verb>");
                    _s_help.AddPreOptionsLine("\r\nList of supported verbs:");
                }
                else
                {
                    _s_help.AddPreOptionsLine("\r\n  ntool.exe " + effectiveVerb + " [options...]");
                    _s_help.AddPreOptionsLine("\r\nList of supported options:");
                }

                if ( this.LastParserState != null && this.LastParserState.Errors != null && this.LastParserState.Errors.Any() )
                { 
                    var errors = _s_help.RenderParsingErrorsText(this, 2); // indent with two spaces

                    if (!string.IsNullOrEmpty(errors))
                    {
                        _s_log.Error("\r\n" + errors);
                    }
                }
                
                return _s_help;
            }
        }
    }
}

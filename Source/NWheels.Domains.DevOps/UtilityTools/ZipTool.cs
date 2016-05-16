using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using NWheels.Logging.Core;
using NWheels.Utilities.Core;

namespace NWheels.Domains.DevOps.UtilityTools
{
    public class ZipTool : UtilityToolBase<ZipTool.Options>
    {
        public const string ToolName = "zip";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ZipTool(IPlainLog log)
            : base(log)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of UtilityToolBase<Options>

        protected override void Execute(Options options)
        {
            ZipFile.CreateFromDirectory(options.FromPath, options.ToPath, CompressionLevel.Optimal, includeBaseDirectory: false);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Options
        {
            [Option('f', "from", Required = true, HelpText = "Input folder to zip")]
            public string FromPath { get; set; }
            
            [Option('t', "to", Required = true, HelpText = "Path to output zip file")]
            public string ToPath { get; set; }
        }
    }
}

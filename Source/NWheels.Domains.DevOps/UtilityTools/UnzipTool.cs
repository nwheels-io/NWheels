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
    public class UnzipTool : UtilityToolBase<UnzipTool.Options>
    {
        public const string ToolName = "unzip";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnzipTool(IPlainLog log)
            : base(log)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of UtilityToolBase<Options>

        protected override void Execute(Options options)
        {
            ZipFile.ExtractToDirectory(options.FromPath, options.ToPath);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Options
        {
            [Option('f', "from", Required = true, HelpText = "Path to zip file")]
            public string FromPath { get; set; }

            [Option('t', "to", Required = true, HelpText = "Folder to extract into")]
            public string ToPath { get; set; }
        }
    }
}

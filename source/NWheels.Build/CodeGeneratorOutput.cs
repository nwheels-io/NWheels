using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetaPrograms;

namespace NWheels.Build
{
    public class CodeGeneratorOutput : ICodeGeneratorOutput
    {
        private readonly BuildOptions _buildOptions;
        private readonly IEnumerable<string> _basePath;

        public CodeGeneratorOutput(BuildOptions buildOptions)
        {
            _buildOptions = buildOptions;
            _basePath = new string[] {
                Path.Combine(Path.GetDirectoryName(buildOptions.ProjectFilePath), "nwheels.build")
            };
        }

        public void AddSourceFile(IEnumerable<string> folderPath, string fileName, string contents)
        {
            var folderPathString = Path.Combine(_basePath.Concat(folderPath).ToArray());
            Directory.CreateDirectory(folderPathString);

            var filePath = Path.Combine(folderPathString, fileName);
            File.WriteAllText(filePath, contents);
        }

        public CodeTextOptions TextOptions { get; }
    }
}
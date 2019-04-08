using System;
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
        private readonly string _basePath;

        public CodeGeneratorOutput(BuildOptions buildOptions)
        {
            _buildOptions = buildOptions;
            _basePath = Path.Combine(Path.GetDirectoryName(buildOptions.ProjectFilePath), "nwheels.build");
        }

        public void AddSourceFile(FilePath path, string contents)
        {
            Console.WriteLine($"- {path.FullPath}");
            
            var absolutePath = path.WithBase(_basePath);
            Directory.CreateDirectory(absolutePath.FolderPath);
            File.WriteAllText(absolutePath.FullPath, contents);
        }

        public CodeTextOptions TextOptions { get; }
    }
}

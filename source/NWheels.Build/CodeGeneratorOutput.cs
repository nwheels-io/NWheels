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
        private readonly Dictionary<string, int> _slocPerFileType;

        public CodeGeneratorOutput(BuildOptions buildOptions)
        {
            _buildOptions = buildOptions;
            _basePath = Path.Combine(Path.GetDirectoryName(buildOptions.ProjectFilePath), "nwheels.build");
            _slocPerFileType = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            
            this.TextOptions = new CodeTextOptions(indent: "    ", newLine: System.Environment.NewLine);
        }

        public void AddSourceFile(FilePath path, string contents)
        {
            Console.WriteLine($"- {path.FullPath}");
            
            var absolutePath = path.WithBase(_basePath);
            Directory.CreateDirectory(absolutePath.FolderPath);
            File.WriteAllText(absolutePath.FullPath, contents);

            IncrementSlocCount(path, contents);
        }
        
        public IReadOnlyDictionary<string, int> GetSlocPerFileType()
        {
            return _slocPerFileType;
        }

        public CodeTextOptions TextOptions { get; }

        private void IncrementSlocCount(FilePath path, string contents)
        {
            var fileType = Path.GetExtension(path.FileName);

            if (fileType == string.Empty || fileType == ".")
            {
                fileType = path.FileName;
            }
            
            var fileLineCount = contents.Count(c => c == '\n');
            var currentLineCount = (_slocPerFileType.TryGetValue(fileType, out var value) ? value : 0);

            _slocPerFileType[fileType] = currentLineCount + fileLineCount;
        }
    }
}

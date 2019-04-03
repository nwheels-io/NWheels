using System.CodeDom.Compiler;
using System.Collections.Generic;
using MetaPrograms;

namespace NWheels.Build
{
    public class CodeGeneratorOutput : ICodeGeneratorOutput
    {
        public void AddSourceFile(IEnumerable<string> folderPath, string fileName, string contents)
        {
            throw new System.NotImplementedException();
        }

        public CodeTextOptions TextOptions { get; }
    }
}
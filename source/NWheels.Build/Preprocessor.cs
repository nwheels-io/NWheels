using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Members;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class Preprocessor
    {
        private readonly PreprocessorOutput _output = new PreprocessorOutput();
        
        public Preprocessor(ImperativeCodeModel code, RoslynCodeModelReader reader)
        {
            this.Code = code;
            this.Reader = reader;
        }

        public IReadOnlyPreprocessorOutput Run()
        {
            throw new NotImplementedException();

            return _output;
        }

        public ImperativeCodeModel Code { get; }
        public RoslynCodeModelReader Reader { get; }
    }
}

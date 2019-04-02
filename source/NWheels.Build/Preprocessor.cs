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
        private readonly Dictionary<TypeMember, MetaElement> _preprocessedMembers = 
            new Dictionary<TypeMember, MetaElement>();
        
        public Preprocessor(RoslynCodeModelReader reader, ImperativeCodeModel code)
        {
            this.Reader = reader;
            this.Code = code;
        }

        public MetaElement GetMetaElement(TypeMember member)
        {
            if (_preprocessedMembers.TryGetValue(member, out var existingElement))
            {
                return existingElement;
            }

            var builder = new MetaElementBuilder(this, member, );

            try
            {
                _preprocessedMembers.Add(member, builder.MetaElement);    
            }
            catch
            {
                _preprocessedMembers.Remove(member);    
                throw;
            }

            return builder.MetaElement;
        }

        public RoslynCodeModelReader Reader { get; }
        public ImperativeCodeModel Code { get; }
    }
}
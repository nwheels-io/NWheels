using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public abstract class SyntaxEmitterBase<TSyntax> : ISyntaxEmitter<TSyntax>, ISyntaxEmitter
        where TSyntax : SyntaxNode
    {
        public abstract TSyntax EmitSyntax();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        SyntaxNode ISyntaxEmitter.EmitSyntax()
        {
            return this.EmitSyntax();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TSyntax OutputSyntax { get; protected set; }
    }
}

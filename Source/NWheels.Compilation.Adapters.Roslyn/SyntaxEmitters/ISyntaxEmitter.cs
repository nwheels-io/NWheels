using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public interface ISyntaxEmitter
    {
        SyntaxNode EmitSyntax(); 
    }

    public interface ISyntaxEmitter<TSyntax>
        where TSyntax : SyntaxNode
    {
        TSyntax EmitSyntax();
    }
}

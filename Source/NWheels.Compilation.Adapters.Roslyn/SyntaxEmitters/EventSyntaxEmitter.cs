using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class EventSyntaxEmitter : MemberSyntaxEmitterBase<EventMember, EventDeclarationSyntax>
    {
        public EventSyntaxEmitter(EventMember @event) :
            base(@event)
        {
        }

        public override EventDeclarationSyntax EmitSyntax()
        {
            throw new NotImplementedException();
        }
    }
}
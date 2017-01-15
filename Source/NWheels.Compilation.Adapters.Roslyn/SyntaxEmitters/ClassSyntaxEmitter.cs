using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters
{
    public class ClassSyntaxEmitter : MemberSyntaxEmitterBase<TypeMember, ClassDeclarationSyntax>
    {
        public ClassSyntaxEmitter(TypeMember member) 
            : base(member)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ClassDeclarationSyntax EmitSyntax()
        {
            OutputSyntax = ClassDeclaration(Member.Name);
            OutputSyntax = OutputSyntax.WithModifiers(EmitVisibilityModifiers());

            if (Member.BaseType != null)
            {
                OutputSyntax = OutputSyntax
                    .WithBaseList(
                        BaseList(
                            SeparatedList<BaseTypeSyntax>(new BaseTypeSyntax[] {
                                SimpleBaseType(
                                    ParseTypeName(Member.BaseType.FullName)
                                )
                            })
                        )
                    );
            }

            return OutputSyntax;
        }
    }
}

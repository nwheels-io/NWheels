using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Conventions
{
    public interface ITypeMemberBuilder
    {
        void ChangeDeclaration(
            string replaceNamespace = null,
            string replaceName = null,
            ITypeMember replaceBaseClass = null,
            IEnumerable<ITypeMember> removeInterfaces = null,
            IEnumerable<ITypeMember> addInterfaces = null,
            IEnumerable<ITypeMember> replaceInterfaces = null,
            MemberVisibility? replaceVisibility = null,
            MemberModifiers? replaceModifiers = null);

        IFieldMember AddField(string name, ITypeMember type, MemberVisibility visibility, MemberModifiers modifiers);
        IConstructorMember AddConstructor(MemberVisibility visibility, MemberModifiers modifiers, ITypeMember[] parameterTypes);
        IMethodMember AddMethod(string name, MemberVisibility visibility, MemberModifiers modifiers, ITypeMember[] parameterTypes);


        ITypeMember Product { get; }
    }
}

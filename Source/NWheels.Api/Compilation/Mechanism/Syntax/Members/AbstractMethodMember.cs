using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public abstract class MethodMemberBase : AbstractMember
    {
        protected MethodMemberBase()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MethodMemberBase(MethodBase clrBinding)
            : base(clrBinding)
        {
            this.Modifier = GetMemberModifier(clrBinding);
            this.Visibility = GetMemberVisibility(clrBinding);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodSignature Signature { get; set; }
        public BlockStatement Body { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static MemberModifier GetMemberModifier(MethodBase binding)
        {
            var modifiers = MemberModifier.None;

            if (binding.IsStatic)
            {
                modifiers |= MemberModifier.Static;
            }

            if (binding.IsAbstract)
            {
                modifiers |= MemberModifier.Abstract;
            }

            if (binding.IsVirtual)
            {
                modifiers |= MemberModifier.Virtual;
            }

            return modifiers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static MemberVisibility GetMemberVisibility(MethodBase binding)
        {
            if (binding.IsPrivate)
            {
                return MemberVisibility.Private;
            }

            if (binding.IsPublic)
            {
                return MemberVisibility.Public;
            }

            if (binding.IsFamilyAndAssembly)
            {
                return MemberVisibility.InternalProtected;
            }

            if (binding.IsFamily)
            {
                return MemberVisibility.Protected;
            }

            if (binding.IsAssembly)
            {
                return MemberVisibility.Internal;
            }

            throw new ArgumentException($"Visibility of member '{binding.Name}' cannot be determined.", nameof(binding));
        }
    }
}

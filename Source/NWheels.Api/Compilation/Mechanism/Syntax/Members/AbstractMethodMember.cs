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


        protected MethodMemberBase(MethodBase binding)
            : base(binding)
        {
            this.Modifiers = GetMemberModifiers(binding);
            this.Visibility = GetMemberVisibility(binding);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodSignature Signature { get; set; }
        public AbstractStatement Body { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static MemberModifiers GetMemberModifiers(MethodBase binding)
        {
            var modifiers = MemberModifiers.None;

            if (binding.IsStatic)
            {
                modifiers |= MemberModifiers.Static;
            }

            if (binding.IsAbstract)
            {
                modifiers |= MemberModifiers.Abstract;
            }

            if (binding.IsVirtual)
            {
                modifiers |= MemberModifiers.Virtual;
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

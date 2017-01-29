using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class TypeMember : AbstractMember, IEquatable<TypeMember>
    {
        public TypeMember()
        {
            this.Interfaces = new HashSet<TypeMember>();
            this.GenericTypeArguments = new List<TypeMember>();
            this.Members = new List<AbstractMember>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(Type compiledType)
            : this()
        {
            this.ClrBinding = compiledType;
            this.Name = compiledType.Name;
            this.Namespace = compiledType.Namespace;

            if (compiledType.DeclaringType != null)
            {
                this.DeclaringType = compiledType.DeclaringType;
            }

            var info = compiledType.GetTypeInfo();

            this.AssemblyName = info.Assembly.FullName;
            this.IsGenericType = info.IsGenericType;

            if (IsGenericType)
            {
                this.GenericTypeArguments.AddRange(info.GenericTypeArguments.Select(t => new TypeMember(t)));
                this.Name = this.Name.Substring(0, this.Name.IndexOf('`'));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(TypeGeneratorInfo generator)
            : this()
        {
            this.Generator = generator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(MemberVisibility visibility, TypeMemberKind typeKind, string name, params TypeMember[] genericTypeArguments)
            : this()
        {
            this.Visibility = visibility;
            this.TypeKind = typeKind;
            this.Name = name;

            if (genericTypeArguments != null)
            {
                this.GenericTypeArguments.AddRange(genericTypeArguments);
            }

            this.IsGenericType = (GenericTypeArguments.Count > 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(string namespaceName, MemberVisibility visibility, TypeMemberKind typeKind, string name, params TypeMember[] genericTypeArguments)
            : this(visibility, typeKind, name, genericTypeArguments)
        {
            this.Namespace = namespaceName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(
            TypeGeneratorInfo generator, 
            string namespaceName, 
            MemberVisibility visibility, 
            TypeMemberKind typeKind, 
            string name, 
            params TypeMember[] genericTypeArguments)
            : this(namespaceName, visibility, typeKind, name, genericTypeArguments)
        {
            this.Generator = generator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(TypeMember other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (this.ClrBinding != null)
            {
                return (this.ClrBinding == other.ClrBinding);
            }

            return ReferenceEquals(this, other);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if (obj is TypeMember other)
            {
                return this.Equals(other);
            }

            return base.Equals(obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            if (ClrBinding != null)
            {
                return 127 ^ ClrBinding.GetHashCode();
            }

            return 17 ^ base.GetHashCode();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return this.FullName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FullName
        {
            get
            {
                if (DeclaringType != null)
                {
                    return DeclaringType.FullName + "." + Name;
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    return Namespace + "." + Name;
                }

                return Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AssemblyName { get; set; }
        public string Namespace { get; set; }
        public TypeMember BaseType { get; set; }
        public HashSet<TypeMember> Interfaces { get; private set; }
        public TypeMemberKind TypeKind { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsValueType { get; set; }
        public bool IsArray { get; set; }
        public bool IsNullable { get; set; }
        public bool IsGenericType { get; set; }
        public bool IsGenericTypeDefinition { get; set; }
        public TypeMember GenericTypeDefinition { get; set; }
        public List<TypeMember> GenericTypeArguments { get; private set; }
        public TypeMember UnderlyingType { get; set; }
        public Type ClrBinding { get; set; }
        public object NonClrBinding { get; set; }
        public object BackendTag { get; set; }
        public List<AbstractMember> Members { get; private set; }
        public TypeGeneratorInfo Generator { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator TypeMember(Type compiledType)
        {
            //TODO: use CompiledTypeMemberCache
            return new TypeMember(compiledType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator == (TypeMember member1, TypeMember member2)
        {
            if (!ReferenceEquals(member1, null))
            {
                return member1.Equals(member2);
            }
            else
            {
                return ReferenceEquals(member2, null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator != (TypeMember member1, TypeMember member2)
        {
            return !(member1 == member2);
        }
    }
}

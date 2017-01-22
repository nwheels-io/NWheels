using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class TypeMember : AbstractMember
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
            this.Binding = compiledType;
            this.Name = compiledType.Name;
            this.Namespace = compiledType.Namespace;

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

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                {
                    return Name;
                }

                return Namespace + "." + Name;
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
        public Type Binding { get; set; }
        public List<AbstractMember> Members { get; private set; }
        public TypeGeneratorInfo Generator { get; private set; }
    }
}

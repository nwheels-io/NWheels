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
            this.GenericTypeParameters = new List<TypeMember>();
            this.Members = new List<AbstractMember>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(Type compiledType)
            : this()
        {
            //System.IO.File.AppendAllText(@"D:\TypeMember.cctor", compiledType.FullName + "\r\n");

            this.ClrBinding = compiledType;
            this.Name = compiledType.Name;
            this.Namespace = compiledType.Namespace;

            //TODO: delay further initialization until values are requested (distinguish runtime from compile-time).

            if (compiledType.DeclaringType != null)
            {
                this.DeclaringType = compiledType.DeclaringType;
            }

            var info = compiledType.GetTypeInfo();

            this.AssemblyName = info.Assembly.FullName;
            this.IsGenericType = info.IsGenericType;

            if (IsGenericType)
            {
                this.IsGenericTypeDefinition = info.IsGenericTypeDefinition;

                if (this.IsGenericTypeDefinition)
                {
                    this.GenericTypeParameters.AddRange(info.GenericTypeParameters.Select(p => new TypeMember() {
                        TypeKind = TypeMemberKind.GenericParameter,
                        Name = p.Name
                    }));
                }
                else
                {
                    this.GenericTypeArguments.AddRange(info.GenericTypeArguments.Select(GetBoundTypeMember));
                    this.GenericTypeDefinition = info.GetGenericTypeDefinition();
                }

                this.Name = this.Name.Substring(0, this.Name.IndexOf('`'));
            }

            if (info.IsArray)
            {
                this.IsArray = true;
                this.IsCollection = true;
                this.UnderlyingType = info.GetElementType();
            }
            else if (GenericTypeDefinition != null && _s_collectionGenericTypeDefinitions.Contains(GenericTypeDefinition.ClrBinding))
            {
                this.IsCollection = true;
                this.UnderlyingType = GenericTypeArguments[0];
            }
            else if (this.IsGenericTypeDefinition && _s_collectionGenericTypeDefinitions.Contains(this.ClrBinding))
            {
                this.IsCollection = true;
                this.UnderlyingType = GenericTypeParameters[0];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(TypeGeneratorInfo generator)
            : this()
        {
            this.Generator = generator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(MemberVisibility visibility, TypeMemberKind typeKind, string name, params TypeMember[] genericArguments)
            : this()
        {
            this.Visibility = visibility;
            this.TypeKind = typeKind;
            this.Name = name;

            if (genericArguments != null)
            {
                this.IsGenericType = (genericArguments.Length > 0);

                if (this.IsGenericType)
                {
                    this.IsGenericTypeDefinition = genericArguments.Any(arg => arg.TypeKind == TypeMemberKind.GenericParameter);

                    if (this.IsGenericTypeDefinition)
                    {
                        this.GenericTypeParameters.AddRange(genericArguments);
                    }
                    else
                    {
                        this.GenericTypeArguments.AddRange(genericArguments);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember(string namespaceName, MemberVisibility visibility, TypeMemberKind typeKind, string name, params TypeMember[] genericArguments)
            : this(visibility, typeKind, name, genericArguments)
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
            return $"{this.TypeKind} {this.Name}";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember MakeGenericType(params TypeMember[] typeArguments)
        {
            if (this.ClrBinding != null && !typeArguments.Any(t => t.ClrBinding == null))
            {
                return ClrBinding.MakeGenericType(typeArguments.Select(t => t.ClrBinding).ToArray());
            }

            var constructedType = new TypeMember(this.Namespace, this.Visibility, this.TypeKind, this.Name, typeArguments);
            constructedType.GenericTypeDefinition = this;
            return constructedType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(MemberVisitor visitor)
        {
            switch (this.TypeKind)
            {
                case TypeMemberKind.Class:
                    visitor.VisitClassType(this);
                    break;
                case TypeMemberKind.Struct:
                    visitor.VisitStructType(this);
                    break;
                case TypeMemberKind.Interface:
                    visitor.VisitInterfaceType(this);
                    break;
                case TypeMemberKind.Enum:
                    visitor.VisitEnumType(this);
                    break;
            }

            foreach (var member in this.Members)
            {
                member.AcceptVisitor(visitor);
            }
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
        public bool IsCollection { get; set; }
        public bool IsArray { get; set; }
        public bool IsNullable { get; set; }
        public bool IsAwaitable { get; set; }
        public bool IsGenericType { get; set; }
        public bool IsGenericTypeDefinition { get; set; }
        public TypeMember GenericTypeDefinition { get; set; }
        public List<TypeMember> GenericTypeArguments { get; }
        public List<TypeMember> GenericTypeParameters { get; }
        public TypeMember UnderlyingType { get; set; }
        public Type ClrBinding { get; set; }
        public object NonClrBinding { get; set; }

        public List<AbstractMember> Members { get; }
        public TypeGeneratorInfo Generator { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ImmutableDictionary<Type, TypeMember> _s_typeMemberByClrType;
        private static readonly ImmutableHashSet<Type> _s_collectionGenericTypeDefinitions = ImmutableHashSet<Type>.Empty.Union(new Type[] {
            typeof(ICollection<>), typeof(IList<>), typeof(ISet<>), typeof(List<>), typeof(HashSet<>)
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static TypeMember()
        {
            //System.IO.File.WriteAllText(@"D:\TypeMember.cctor", "starting\r\n");

            try
            { 
                var systemTypes = new Type[] {
                    typeof(bool), typeof(char), typeof(string), typeof(object), typeof(DateTime), typeof(TimeSpan),
                    typeof(byte) , typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
                    typeof(double), typeof(float), typeof(decimal),
                    typeof(Array), typeof(List<>), typeof(HashSet<>), typeof(Dictionary<,>), typeof(IList<>), typeof(ISet<>), typeof(IDictionary<,>)
                };

                _s_typeMemberByClrType = ImmutableDictionary<Type, TypeMember>.Empty;

                AddClrBoundTypeMembers(systemTypes);
                AddClrBoundTypeMembers(systemTypes.Select(t => t.MakeArrayType()));
                AddClrBoundTypeMembers(systemTypes.Select(t => typeof(List<>).MakeGenericType(t)));
                AddClrBoundTypeMembers(systemTypes.Select(t => typeof(IList<>).MakeGenericType(t)));
            }
            catch (Exception e)
            {
                System.IO.File.WriteAllText(@"D:\TypeMember.crash.log", e.ToString());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //-- Thread safety: multiple readers single writer
        public static implicit operator TypeMember(Type clrType)
        {
            return GetBoundTypeMember(clrType);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AddClrBoundTypeMembers(IEnumerable<Type> clrTypes)
        {
            TypeMember GetOrCreateNewTypeMember(Type clrType)
            {
                if (_s_typeMemberByClrType.TryGetValue(clrType, out TypeMember existing))
                {
                    return existing;
                }

                return new TypeMember(clrType);
            }

            var boundClrTypeMemberPairs = clrTypes
                .Select(t => new KeyValuePair<Type, TypeMember>(t, GetOrCreateNewTypeMember(t)))
                .ToList();

            _s_typeMemberByClrType = _s_typeMemberByClrType.AddRange(
                boundClrTypeMemberPairs
                .Where(kvp => !_s_typeMemberByClrType.ContainsKey(kvp.Key)));
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TypeMember GetBoundTypeMember(Type clrType)
        {
            if (clrType == null)
            {
                return null;
            }

            if (clrType.IsByRef)
            {
                clrType = clrType.GetElementType();
            }

            if (_s_typeMemberByClrType.TryGetValue(clrType, out TypeMember existingTypeMember))
            {
                return existingTypeMember;
            }

            var newTypeMember = new TypeMember(clrType);
            _s_typeMemberByClrType = _s_typeMemberByClrType.Add(clrType, newTypeMember);
            return newTypeMember;
        }
    }
}

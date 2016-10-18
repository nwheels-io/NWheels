using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hapil;
using NWheels.Extensions;

namespace NWheels.Serialization
{
    public abstract class CompactSerializerDictionary
    {
        public abstract bool RegisterMember(MemberInfo member, int key);
        public abstract bool RegisterMember(MemberInfo member, out int key);
        public abstract bool TryLookupMember(int key, out MemberInfo member);
        public abstract bool TryLookupMemberKey(MemberInfo member, out int key);
        public abstract bool RegisterType(Type type, int key);
        public abstract bool RegisterType(Type type, out int key);
        public abstract bool TryLookupType(int key, out Type type);
        public abstract bool TryLookupTypeKey(Type type, out int key);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool RegisterMember(MemberInfo member)
        {
            int key;
            return RegisterMember(member, out key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool RegisterType(Type type)
        {
            int key;
            return RegisterType(type, out key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterApiContract(Type type)
        {
            foreach (var member in type.GetMembers().Where(IsProtocolMember))
            {
                RegisterMember(member);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual MemberInfo LookupMemberOrThrow(int key)
        {
            MemberInfo member;

            if (TryLookupMember(key, out member))
            {
                return member;
            }

            throw new CompactSerializerException("Serializer dictionary has no member registered under the key [{0}].", key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual int LookupMemberKeyOrThrow(MemberInfo member)
        {
            int key;

            if (TryLookupMemberKey(member, out key))
            {
                return key;
            }

            throw new CompactSerializerException(
                "Serializer dictionary has no registration for member [{0}.{1}].", 
                member.DeclaringType.FriendlyName(), member.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Type LookupTypeOrThrow(int key)
        {
            Type type;

            if (TryLookupType(key, out type))
            {
                return type;
            }

            throw new CompactSerializerException("Serializer dictionary has no type registered under the key [{0}].", key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Type LookupTypeOrThrow(int key, Type ancestor)
        {
            var type = LookupTypeOrThrow(key);

            if (ancestor.IsAssignableFrom(type))
            {
                return type;
            }

            throw new CompactSerializerException("Type registered under the key [{0}] does not derive from type '{1}'.", key, type.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual int LookupTypeKeyOrThrow(Type type)
        {
            int key;

            if (TryLookupTypeKey(type, out key))
            {
                return key;
            }

            throw new CompactSerializerException(
                "Serializer dictionary has no registration for type [{0}].",
                type.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool ShouldWriteTypeKey(object obj, Type declaredType, Type resolvedSerializationType, out int key)
        {
            if (resolvedSerializationType != declaredType)
            {
                key = this.LookupTypeKeyOrThrow(resolvedSerializationType);
                return true;
            }
            else
            {
                key = -1;
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void WriteTo(CompactBinaryWriter writer);
        public abstract void ReadFrom(CompactBinaryReader reader);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void WriteTo(CompactBinaryWriter writer, SerializableTypeEntry[] typeEntries, SerializableMemberEntry[] memberEntries)
        {
            writer.WriteArray(
                typeEntries,
                (w, entry, ctx) => entry.WriteTo(w),
                context: null);

            writer.WriteArray(
                memberEntries, 
                (w, entry, ctx) => entry.WriteTo(w), 
                context: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ReadFrom(CompactBinaryReader reader, out SerializableTypeEntry[] typeEntries, out SerializableMemberEntry[] memberEntries)
        {
            typeEntries = reader.ReadArray(
                (r, ctx) => new SerializableTypeEntry(r),
                context: null);

            memberEntries = reader.ReadArray(
                (r, ctx) => new SerializableMemberEntry(r),
                context: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected bool IsProtocolMember(MemberInfo member)
        {
            var methodInfo = member as MethodInfo;

            if (methodInfo != null)
            {
                return ((methodInfo.Attributes & MethodAttributes.SpecialName) == 0);
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected struct SerializableTypeEntry
        {
            public SerializableTypeEntry(int key, Type type)
            {
                Key = key;
                FullName = type.FullName;
                AssemblyName = type.Assembly.GetName().Name;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SerializableTypeEntry(CompactBinaryReader reader)
            {
                Key = reader.Read7BitInt();
                FullName = reader.ReadString();
                AssemblyName = reader.ReadString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WriteTo(CompactBinaryWriter writer)
            {
                writer.Write7BitInt(Key);
                writer.Write(FullName);
                writer.Write(AssemblyName);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type DeserializeType(CompactSerializerDictionary dictionary)
            {
                var type = Type.GetType(FullName + ", " + AssemblyName, throwOnError: false);

                if (type != null)
                {
                    return type;
                }

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == AssemblyName)
                    {
                        type = Type.GetType(FullName + ", " + assembly.FullName, throwOnError: false);
                    }
                }

                if (type != null)
                {
                    return type;
                }

                throw new CompactSerializerException("Cannot find type '{0}, {1}' in current AppDomain.", FullName, AssemblyName);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly int Key;
            public readonly string FullName;
            public readonly string AssemblyName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected struct SerializableMemberEntry
        {
            public SerializableMemberEntry(int key, int declaringTypeKey, string name)
            {
                Key = key;
                DeclaringTypeKey = declaringTypeKey;
                Name = name;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SerializableMemberEntry(CompactBinaryReader reader)
            {
                Key = reader.Read7BitInt();
                DeclaringTypeKey = reader.Read7BitInt();
                Name = reader.ReadString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WriteTo(CompactBinaryWriter writer)
            {
                writer.Write7BitInt(Key);
                writer.Write7BitInt(DeclaringTypeKey);
                writer.Write(Name);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MemberInfo DeserializeMember(CompactSerializerDictionary dictionary)
            {
                var declaringType = dictionary.LookupTypeOrThrow(DeclaringTypeKey);
                var members = declaringType.GetMember(Name);

                if (members.Length == 0)
                {
                    throw new CompactSerializerException(
                        "Member [{0}.{1}] cannot be found in current AppDomain.", 
                        declaringType.FullName, Name);
                }

                if (members.Length > 1)
                {
                    throw new CompactSerializerException(
                        "Ambiguous match for member [{0}.{1}]. Overloaded members are not supported.", 
                        declaringType.FullName, Name);
                }

                return members[0];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly int Key;
            public readonly int DeclaringTypeKey;
            public readonly string Name;
        }
    }
}

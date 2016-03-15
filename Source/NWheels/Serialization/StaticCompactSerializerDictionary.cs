using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NWheels.Serialization
{
    public class StaticCompactSerializerDictionary : CompactSerializerDictionary
    {
        private readonly Dictionary<MemberInfo, int> _keyByMember;
        private readonly Dictionary<Type, int> _keyByType;
        private readonly Dictionary<int, MemberInfo> _memberByKey;
        private readonly Dictionary<int, Type> _typeByKey;
        private volatile bool _immutable;
        private int _nextKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StaticCompactSerializerDictionary()
        {
            _keyByMember = new Dictionary<MemberInfo, int>();
            _keyByType = new Dictionary<Type, int>();
            _memberByKey = new Dictionary<int, MemberInfo>();
            _typeByKey = new Dictionary<int, Type>();
            _nextKey = 1;
            _immutable = false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void MakeImmutable()
        {
            _immutable = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool RegisterMember(MemberInfo member, int key)
        {
            ValidateMutability();

            if (!_keyByMember.ContainsKey(member))
            {
                int declaringTypeKey;
                RegisterType(member.DeclaringType, out declaringTypeKey);

                _memberByKey.Add(key, member);
                _keyByMember.Add(member, key);
                
                return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool RegisterMember(MemberInfo member, out int key)
        {
            ValidateMutability();

            if (_keyByMember.TryGetValue(member, out key))
            {
                return false;
            }

            int declaringTypeKey;
            RegisterType(member.DeclaringType, out declaringTypeKey);

            key = _nextKey++;
            
            _memberByKey.Add(key, member);
            _keyByMember.Add(member, key);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool RegisterType(Type type, int key)
        {
            ValidateMutability();

            if (!_keyByType.ContainsKey(type))
            {
                _typeByKey.Add(key, type);
                _keyByType.Add(type, key);

                return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool RegisterType(Type type, out int key)
        {
            ValidateMutability();

            if (_keyByType.TryGetValue(type, out key))
            {
                return false;
            }

            key = _nextKey++;

            _typeByKey.Add(key, type);
            _keyByType.Add(type, key);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool TryLookupMember(int key, out MemberInfo member)
        {
            return _memberByKey.TryGetValue(key, out member);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool TryLookupMemberKey(MemberInfo member, out int key)
        {
            return _keyByMember.TryGetValue(member, out key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool TryLookupType(int key, out Type type)
        {
            return _typeByKey.TryGetValue(key, out type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool TryLookupTypeKey(Type type, out int key)
        {
            return _keyByType.TryGetValue(type, out key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override void WriteTo(CompactBinaryWriter writer)
        {
            var typeEntries = _typeByKey.Select(kvp => new SerializableTypeEntry(kvp.Key, kvp.Value)).ToArray();
            var memberEntries = _memberByKey.Select(kvp => new SerializableMemberEntry(
                key: kvp.Key, 
                declaringTypeKey: LookupTypeKeyOrThrow(kvp.Value.DeclaringType), 
                name: kvp.Value.Name
            )).ToArray();

            WriteTo(writer, typeEntries, memberEntries);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ReadFrom(CompactBinaryReader reader)
        {
            ValidateMutability();

            _keyByType.Clear();
            _typeByKey.Clear();
            _keyByMember.Clear();
            _memberByKey.Clear();

            SerializableTypeEntry[] typeEntries;
            SerializableMemberEntry[] memberEntries;
            ReadFrom(reader, out typeEntries, out memberEntries);

            foreach (var entry in typeEntries)
            {
                var type = entry.DeserializeType(this);
                
                _typeByKey.Add(entry.Key, type);
                _keyByType.Add(type, entry.Key);
            }

            foreach (var entry in memberEntries)
            {
                var member = entry.DeserializeMember(this);

                _memberByKey.Add(entry.Key, member);
                _keyByMember.Add(member, entry.Key);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void ValidateMutability()
        {
            if (_immutable)
            {
                throw new InvalidOperationException("This serializer dictionary is immutable.");
            }
        }
    }
}
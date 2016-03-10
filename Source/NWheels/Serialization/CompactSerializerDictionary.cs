using System;
using System.Collections.Generic;
using System.Reflection;

namespace NWheels.Serialization
{
    public class CompactSerializerDictionary
    {
        //private readonly ConcurrentDictionary<int, MemberInfo> _memberByKey;
        //private readonly ConcurrentDictionary<int, Type> _typeByKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterMember(MethodInfo member, int key)
        {
            MemberRegistered(null, new KeyValuePair<int, MemberInfo>());
            TypeRegistered(null, new KeyValuePair<int, Type>());
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterMember(MethodInfo member, out int key)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterType(Type type, int key)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterType(Type type, out int key)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryLookupMember(int key, out MemberInfo member)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryLookupType(int key, out Type type)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MemberInfo LookupMemberOrThrow(int key)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type LookupTypeOrThrow(int key)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type LookupTypeOrThrow(int key, Type ancestor)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ShouldWriteTypeKey(object obj, Type declaredType, Type resolvedSerializationType, out int key)
        {
            key = -1;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<KeyValuePair<int, MemberInfo>> MemberRegistered;
        public event EventHandler<KeyValuePair<int, Type>> TypeRegistered;
    }
}

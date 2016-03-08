using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Serialization
{
    public class CompactSerializationContext
    {
        public CompactSerializationContext(ObjectCompactSerializer serializer, ObjectCompactSerializerDictionary dictionary, CompactBinaryWriter output)
        {
            Serializer = serializer;
            Dictionary = dictionary;
            Output = output;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject(Type declaredType, object obj)
        {
            Serializer.WriteObject(declaredType, obj, this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCompactSerializer Serializer { get; private set; }
        public ObjectCompactSerializerDictionary Dictionary { get; private set; }
        public CompactBinaryWriter Output { get; private set; }
    }
}

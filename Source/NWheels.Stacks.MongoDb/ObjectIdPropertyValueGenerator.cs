using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using MongoDB.Bson;
using NWheels.DataObjects.Core;

namespace NWheels.Stacks.MongoDb
{
    public class ObjectIdPropertyValueGenerator : IPropertyValueGenerator<ObjectId>, IPropertyValueGeneratorWriter
    {
        #region Implementation of IPropertyValueGenerator<ObjectId>

        public ObjectId GenerateValue(string propertyQualifiedName)
        {
            return ObjectId.GenerateNewId();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IPropertyValueGeneratorWriter

        public void WriteGenerateValue<T>(string propertyQualifiedName, MethodWriterBase method, MutableOperand<T> destination)
        {
            destination.Assign(Static.Func(ObjectId.GenerateNewId).CastTo<T>());
        }

        #endregion
    }
}

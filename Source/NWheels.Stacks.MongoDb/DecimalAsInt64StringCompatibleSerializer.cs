using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using NWheels.TypeModel.Core.StorageTypes;

namespace NWheels.Stacks.MongoDb
{
    public class DecimalAsInt64StringCompatibleSerializer : Int64Serializer
    {
        #region Overrides of Int64Serializer

        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.GetCurrentBsonType();
            
            switch (bsonType)
            {
                case BsonType.String:
                    var decimalValue = XmlConvert.ToDecimal(bsonReader.ReadString());
                    var int64EncodedValue = DecimalAsInt64StorageType.EncodeDecimalAsInt64(
                        decimalValue, 
                        decimalDigits: 6); //TODO: number of decimal digits must be configured by storage data type applied to property
                    return int64EncodedValue;
            }

            return base.Deserialize(bsonReader, nominalType, actualType, options);
        }

        #endregion
    }
}

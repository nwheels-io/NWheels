using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Processing.Messages;
using NWheels.TypeModel.Factories;

namespace NWheels.TypeModel.Serialization
{
    public class ObjectCompactSerializer
    {
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ObjectCompactReaderWriterFactory _readerWriterFactory;
        private readonly IObjectTypeResolver _typeResolver;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCompactSerializer(IComponentContext components, ITypeMetadataCache metadataCache, ObjectCompactReaderWriterFactory readerWriterFactory)
            : this(components, metadataCache, readerWriterFactory, new VoidTypeResolver())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCompactSerializer(
            IComponentContext components,
            ITypeMetadataCache metadataCache,
            ObjectCompactReaderWriterFactory readerWriterFactory,
            IObjectTypeResolver typeResolver)
        {
            _components = components;
            _metadataCache = metadataCache;
            _readerWriterFactory = readerWriterFactory;
            _typeResolver = typeResolver;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType, Stream input, ObjectCompactSerializerDictionary dictionary)
        {
            return ReadObject(declaredType, new CompactBinaryReader(input), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType, byte[] input, ObjectCompactSerializerDictionary dictionary)
        {
            return ReadObject(declaredType, new CompactBinaryReader(new MemoryStream(input)), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType, CompactBinaryReader input, ObjectCompactSerializerDictionary dictionary)
        {
            var indicatorByte = input.ReadByte();
            Type serializedType;

            switch (indicatorByte)
            {
                case ObjectIndicatorByte.Null:
                    return null;
                case ObjectIndicatorByte.NotNull:
                    serializedType = declaredType;
                    break;
                case ObjectIndicatorByte.NotNullWithTypeKey:
                    var serializedTypeKey = input.ReadInt16();
                    serializedType = dictionary.LookupTypeOrThrow(serializedTypeKey, ancestor: declaredType);
                    break;
                default:
                    throw new InvalidDataException(string.Format("Input stream is invalid: object indicator byte={0}.", indicatorByte));
            }

                    actualType = _typeResolver.GetDeserializationType(declaredType, serializedType);

            if (_typeResolver.CanMaterialize(declaredType, ))

            var creator = _readerWriterFactory.GetDefaultCreator(actualType);
            var reader = _readerWriterFactory.GetReader(actualType);
            var obj = creator(_components);

            reader(this, input, dictionary, obj);

            return obj;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject(Type declaredType, object obj, Stream output, ObjectCompactSerializerDictionary dictionary)
        {
            WriteObject(declaredType, obj, new CompactBinaryWriter(output), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public byte[] WriteObject(Type declaredType, object obj, ObjectCompactSerializerDictionary dictionary)
        {
            byte[] serializedBytes;

            using (var output = new MemoryStream())
            {
                WriteObject(declaredType, obj, new CompactBinaryWriter(output), dictionary);
                serializedBytes = output.ToArray();
            }

            return serializedBytes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject(Type declaredType, object obj, CompactBinaryWriter output, ObjectCompactSerializerDictionary dictionary)
        {
            if (obj == null)
            {
                output.Write(ObjectIndicatorByte.Null);
                return;
            }

            var resolvedSerializationType = _typeResolver.GetSerializationType(declaredType, obj);

            int typeKey;
            if (dictionary.ShouldWriteTypeKey(obj, declaredType, resolvedSerializationType, out typeKey))
            {
                output.Write(ObjectIndicatorByte.NotNullWithTypeKey);
                output.Write((Int16)typeKey);
            }
            else
            {
                output.Write(ObjectIndicatorByte.NotNull);
            }

            var writer = _readerWriterFactory.GetWriter(obj.GetType());
            writer(this, output, dictionary, obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class VoidTypeResolver : IObjectTypeResolver
        {
            public Type GetSerializationType(Type declaredType, object obj)
            {
                return declaredType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type GetDeserializationType(Type declaredType, Type serializedType)
            {
                return serializedType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanMaterialize(Type declaredType, Type serializedType)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Materialize(Type declaredType, Type serializedType)
            {
                throw new NotSupportedException();
            }
        }
    }
}

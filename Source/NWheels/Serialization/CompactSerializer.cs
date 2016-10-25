using System;
using System.IO;
using Autofac;
using NWheels.DataObjects;
using NWheels.Serialization.Factories;

namespace NWheels.Serialization
{
    public class CompactSerializer
    {
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly CompactSerializerFactory _readerWriterFactory;
        private readonly Pipeline<ICompactSerializerExtension> _extensions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializer(
            IComponentContext components,
            ITypeMetadataCache metadataCache,
            CompactSerializerFactory readerWriterFactory,
            Pipeline<ICompactSerializerExtension> extensions = null)
        {
            _components = components;
            _metadataCache = metadataCache;
            _readerWriterFactory = readerWriterFactory;
            _extensions = extensions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object GetObject(Type declaredType, byte[] serializedBytes, CompactSerializerDictionary dictionary)
        {
            return ReadObject(declaredType, new CompactBinaryReader(new MemoryStream(serializedBytes)), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType, Stream input, CompactSerializerDictionary dictionary)
        {
            return ReadObject(declaredType, new CompactBinaryReader(input), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ReadObject(Type declaredType, CompactBinaryReader input, CompactSerializerDictionary dictionary)
        {
            return ReadObject(declaredType, new CompactDeserializationContext(this, dictionary, input, _components));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetObject<T>(byte[] serializedBytes, CompactSerializerDictionary dictionary)
        {
            return (T)ReadObject(typeof(T), new CompactBinaryReader(new MemoryStream(serializedBytes)), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T ReadObject<T>(Stream input, CompactSerializerDictionary dictionary)
        {
            return (T)ReadObject(typeof(T), new CompactBinaryReader(input), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T ReadObject<T>(CompactBinaryReader input, CompactSerializerDictionary dictionary)
        {
            return (T)ReadObject(typeof(T), new CompactDeserializationContext(this, dictionary, input, _components));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PopulateObject(Stream input, CompactSerializerDictionary dictionary, object instance)
        {
            PopulateObject(new CompactBinaryReader(input), dictionary, instance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PopulateObject(CompactBinaryReader input, CompactSerializerDictionary dictionary, object instance)
        {
            PopulateObject(new CompactDeserializationContext(this, dictionary, input, _components), instance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public byte[] GetBytes(Type declaredType, object obj, CompactSerializerDictionary dictionary)
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

        public void WriteObject(Type declaredType, object obj, Stream output, CompactSerializerDictionary dictionary)
        {
            WriteObject(declaredType, obj, new CompactBinaryWriter(output), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject(Type declaredType, object obj, CompactBinaryWriter output, CompactSerializerDictionary dictionary)
        {
            WriteObject(declaredType, obj, new CompactSerializationContext(this, dictionary, output));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObjectContents(object obj, Stream output, CompactSerializerDictionary dictionary)
        {
            WriteObjectContents(obj, new CompactBinaryWriter(output), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObjectContents(object obj, CompactBinaryWriter output, CompactSerializerDictionary dictionary)
        {
            WriteObjectContents(obj, new CompactSerializationContext(this, dictionary, output));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public byte[] GetBytes<T>(T obj, CompactSerializerDictionary dictionary)
        {
            byte[] serializedBytes;

            using (var output = new MemoryStream())
            {
                WriteObject(typeof(T), obj, new CompactBinaryWriter(output), dictionary);
                serializedBytes = output.ToArray();
            }

            return serializedBytes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject<T>(T obj, Stream output, CompactSerializerDictionary dictionary)
        {
            WriteObject(typeof(T), obj, new CompactBinaryWriter(output), dictionary);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteObject<T>(T obj, CompactBinaryWriter output, CompactSerializerDictionary dictionary)
        {
            WriteObject(typeof(T), obj, new CompactSerializationContext(this, dictionary, output));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal object ReadObject(Type declaredType, CompactDeserializationContext context)
        {
            ValidateReferenceType(declaredType);

            var input = context.Input;
            var dictionary = context.Dictionary;

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

            object materializedInstance;
            var materializer = TryFindMaterializingExtension(declaredType, serializedType);

            if (materializer != null)
            {
                materializedInstance = materializer.Materialize(declaredType, serializedType);
            }
            else
            {
                var materializationType = GetMaterializationType(declaredType, serializedType);
                var creator = _readerWriterFactory.GetDefaultCreator(materializationType);
                materializedInstance = creator(context);
            }

            PopulateObject(context, materializedInstance);
            return materializedInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void PopulateObject(CompactDeserializationContext context, object instance)
        {
            var reader = _readerWriterFactory.GetTypeReader(instance.GetType());
            reader(context, instance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void WriteObject(Type declaredType, object obj, CompactSerializationContext context)
        {
            ValidateReferenceType(declaredType);

            var output = context.Output;
            var dictionary = context.Dictionary;

            if (obj == null)
            {
                output.Write(ObjectIndicatorByte.Null);
                return;
            }

            var resolvedSerializationType = GetSerializationType(declaredType, obj);

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

            WriteObjectContents(obj, context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void WriteObjectContents(object obj, CompactSerializationContext context)
        {
            var writer = _readerWriterFactory.GetTypeWriter(obj.GetType());
            writer(context, obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void WriteStruct<T>(ref T value, CompactSerializationContext context) where T : struct
        {
            var writer = _readerWriterFactory.GetStructTypeWriter<T>();
            writer(context, ref value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal T ReadStruct<T>(CompactDeserializationContext context) where T : struct
        {
            var reader = _readerWriterFactory.GetStructTypeReader<T>();
            var creator = _readerWriterFactory.GetStructDefaultCreator<T>();
            T value = creator(context);
            reader(context, ref value);
            return value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetSerializationType(Type declaredType, object obj)
        {
            if (_extensions != null)
            {
                for (int i = 0 ; i < _extensions.Count ; i++)
                {
                    var serializationType = _extensions[i].GetSerializationType(declaredType, obj);

                    if (serializationType != obj.GetType())
                    {
                        return serializationType;
                    }
                }
            }

            return obj.GetType();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetMaterializationType(Type declaredType, Type serializedType)
        {
            if (_extensions != null)
            {
                for (int i = 0 ; i < _extensions.Count ; i++)
                {
                    var materiaizationType = _extensions[i].GetMaterializationType(declaredType, serializedType);

                    if (materiaizationType != serializedType)
                    {
                        return materiaizationType;
                    }
                }
            }

            return serializedType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ICompactSerializerExtension TryFindMaterializingExtension(Type declaredType, Type serializedType)
        {
            if (_extensions != null)
            {
                for (int i = 0 ; i < _extensions.Count ; i++)
                {
                    if (_extensions[i].CanMaterialize(declaredType, serializedType))
                    {
                        return _extensions[i];
                    }
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateReferenceType(Type declaredType)
        {
            if (declaredType.IsValueType)
            {
                throw new ArgumentException("Must be a reference type.", "declaredType");
            }
        }
    }
}

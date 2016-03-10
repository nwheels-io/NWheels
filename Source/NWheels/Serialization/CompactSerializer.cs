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
        private readonly Pipeline<IObjectTypeResolver> _typeResolvers;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializer(IComponentContext components, ITypeMetadataCache metadataCache, CompactSerializerFactory readerWriterFactory)
            : this(components, metadataCache, readerWriterFactory, new IObjectTypeResolver[] { new VoidTypeResolver() })
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializer(
            IComponentContext components,
            ITypeMetadataCache metadataCache,
            CompactSerializerFactory readerWriterFactory,
            Pipeline<IObjectTypeResolver> typeResolvers)
        {
            _components = components;
            _metadataCache = metadataCache;
            _readerWriterFactory = readerWriterFactory;
            _typeResolvers = typeResolvers;
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
            return ReadObject(declaredType, new CompactDeserializationContext(this, dictionary, input));
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
            return (T)ReadObject(typeof(T), new CompactDeserializationContext(this, dictionary, input));
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
            var materializer = TryFindMaterializingResolver(declaredType, serializedType);

            if (materializer != null)
            {
                materializedInstance = materializer.Materialize(declaredType, serializedType);
            }
            else
            {
                var materializationType = GetDeserializationType(declaredType, serializedType);
                var creator = _readerWriterFactory.GetDefaultCreator(materializationType);
                materializedInstance = creator(_components);
            }

            var reader = _readerWriterFactory.GetTypeReader(materializedInstance.GetType());
            reader(context, materializedInstance);

            return materializedInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void WriteObject(Type declaredType, object obj, CompactSerializationContext context)
        {
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

            var writer = _readerWriterFactory.GetTypeWriter(obj.GetType());
            writer(context, obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetSerializationType(Type declaredType, object obj)
        {
            for (int i = 0 ; i < _typeResolvers.Count ; i++)
            {
                var serializationType = _typeResolvers[i].GetSerializationType(declaredType, obj);

                if (serializationType != declaredType)
                {
                    return serializationType;
                }
            }

            return declaredType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetDeserializationType(Type declaredType, Type serializedType)
        {
            for (int i = 0; i < _typeResolvers.Count; i++)
            {
                var deserializationType = _typeResolvers[i].GetDeserializationType(declaredType, serializedType);

                if (deserializationType != declaredType)
                {
                    return deserializationType;
                }
            }

            return declaredType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IObjectTypeResolver TryFindMaterializingResolver(Type declaredType, Type serializedType)
        {
            for (int i = 0; i < _typeResolvers.Count; i++)
            {
                if (_typeResolvers[i].CanMaterialize(declaredType, serializedType))
                {
                    return _typeResolvers[i];
                }
            }

            return null;
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
                return declaredType;
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

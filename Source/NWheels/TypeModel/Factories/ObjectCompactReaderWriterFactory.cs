using System;
using System.Collections.Concurrent;
using System.Linq;
using Autofac;
using Hapil;
using NWheels.DataObjects;
using NWheels.TypeModel.Serialization;

namespace NWheels.TypeModel.Factories
{
    public class ObjectCompactReaderWriterFactory
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly DynamicMethodCompiler _compiler;
        private readonly ConcurrentDictionary<Type, Reader> _readerByType;
        private readonly ConcurrentDictionary<Type, Writer> _writerByType;
        private readonly ConcurrentDictionary<Type, Creator> _defaultCreatorByType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCompactReaderWriterFactory(ITypeMetadataCache metadataCache, DynamicModule dynamicModule)
        {
            _metadataCache = metadataCache;
            _compiler = new DynamicMethodCompiler(dynamicModule);
            _readerByType = new ConcurrentDictionary<Type, Reader>();
            _writerByType = new ConcurrentDictionary<Type, Writer>();
            _defaultCreatorByType = new ConcurrentDictionary<Type, Creator>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Reader GetReader(Type type)
        {
            return _readerByType.GetOrAdd(type, CompileReader);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Writer GetWriter(Type type)
        {
            return _writerByType.GetOrAdd(type, CompileWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Creator GetDefaultCreator(Type type)
        {
            return _defaultCreatorByType.GetOrAdd(type, k => CompileDefaultCreator(k));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void Reader(ObjectCompactSerializer serializer, CompactBinaryReader input, ObjectCompactSerializerDictionary dictionary, object obj);
        public delegate void Writer(ObjectCompactSerializer serializer, CompactBinaryWriter output, ObjectCompactSerializerDictionary dictionary, object obj);
        public delegate object Creator(IComponentContext components);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Reader CompileReader(Type type)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Writer CompileWriter(Type type)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Creator CompileDefaultCreator(Type type)
        {
            var constructor = type.GetConstructors().OrderBy(c => c.GetParameters().Length).FirstOrDefault();

            if (constructor == null)
            {
                throw new ArgumentException("Specified type has no public constructors");
            }

            var parameters = constructor.GetParameters();
            var compiledFunc = _compiler.CompileStaticFunction<IComponentContext, object>("DefaultCreator_" + type.FullName,
                (w, components) => {
                    
                });

            return new Creator(compiledFunc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
    }
}

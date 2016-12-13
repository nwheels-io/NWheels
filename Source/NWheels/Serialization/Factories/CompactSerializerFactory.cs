using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.Serialization.Factories
{
    public class CompactSerializerFactory : ConventionObjectFactory
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly DynamicMethodCompiler _compiler;
        private readonly ConcurrentDictionary<Type, ITypeSerializer> _serializerByType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializerFactory(ITypeMetadataCache metadataCache, DynamicModule dynamicModule)
            : base(dynamicModule)
        {
            _metadataCache = metadataCache;
            _compiler = new DynamicMethodCompiler(dynamicModule);
            _serializerByType = new ConcurrentDictionary<Type, ITypeSerializer>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeReader GetTypeReader(Type type)
        {
            var serializer = _serializerByType.GetOrAdd(type, CreateTypeSerializer);
            return serializer.ReadObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeWriter GetTypeWriter(Type type)
        {
            var serializer = _serializerByType.GetOrAdd(type, CreateTypeSerializer);
            return serializer.WriteObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeCreator GetDefaultCreator(Type type)
        {
            var serializer = _serializerByType.GetOrAdd(type, CreateTypeSerializer);
            return serializer.CreateObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StructTypeReader<T> GetStructTypeReader<T>() where T : struct
        {
            var serializer = (ITypeSerializer<T>)_serializerByType.GetOrAdd(typeof(T), CreateTypeSerializer);
            return serializer.ReadObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StructTypeWriter<T> GetStructTypeWriter<T>() where T : struct
        {
            var serializer = (ITypeSerializer<T>)_serializerByType.GetOrAdd(typeof(T), CreateTypeSerializer);
            return serializer.WriteObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StructTypeCreator<T> GetStructDefaultCreator<T>() where T : struct
        {
            var serializer = (ITypeSerializer<T>)_serializerByType.GetOrAdd(typeof(T), CreateTypeSerializer);
            return serializer.CreateObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var forType = context.TypeKey.PrimaryInterface;

            return new IObjectFactoryConvention[] {
                new ReferenceTypeSerializerConvention(this, forType), 
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ITypeSerializer CreateTypeSerializer(Type type)
        {
            if (IsNonPrimitiveStructType(type))
            {
                var builder = new StructTypeSerializerFactory(this, type);
                return builder.CompileStructTypeSerializer();
            }
            else
            {
                var typeEntry = GetOrBuildType(new TypeKey(primaryInterface: type));
                return typeEntry.CreateInstance<ITypeSerializer>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DynamicMethodCompiler Compiler
        {
            get { return _compiler; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsNonPrimitiveStructType(Type type)
        {
            return (type.IsValueType && !type.IsPrimitive && !type.IsEnum && !_s_valueReaderByType.ContainsKey(type));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsSerializableProperty(PropertyInfo property)
        {
            return (
                property.CanRead && 
                property.CanWrite && 
                property.GetMethod.IsPublic && 
                property.SetMethod.IsPublic && 
                property.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueWriter GetValueWriter(Type type)
        {
            ValueWriter writer;

            if (TryGetValueWriter(type, out writer))
            {
                return writer;
            }

            throw new CompactSerializerException("Value of type '{0}' cannot be serialized.", type.FullName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueWriter GetValueWriter(PropertyInfo property)
        {
            ValueWriter writer;

            if (TryGetValueWriter(property.PropertyType, out writer))
            {
                return writer;
            }

            throw new CompactSerializerException(
                "Cannot serialize property '{0}.{1}'. Cannot serialize values of type '{2}'.",
                property.DeclaringType.Name, property.Name, property.PropertyType.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueWriter GetValueWriter(FieldInfo field)
        {
            ValueWriter writer;

            if (TryGetValueWriter(field.FieldType, out writer))
            {
                return writer;
            }

            throw new CompactSerializerException(
                "Cannot serialize field '{0}.{1}'. Cannot serialize values of type '{2}'.",
                field.DeclaringType.Name, field.Name, field.FieldType.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool TryGetValueWriter(Type type, out ValueWriter writer)
        {
            var key = GetValueReaderWriterKey(type);
            return _s_valueWriterByType.TryGetValue(key, out writer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueReader GetValueReader(Type type)
        {
            ValueReader reader;

            if (TryGetValueReader(type, out reader))
            {
                return reader;
            }

            throw new CompactSerializerException("Value of type '{0}' cannot be deserialized.", type.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueReader GetValueReader(PropertyInfo property)
        {
            ValueReader reader;

            if (TryGetValueReader(property.PropertyType, out reader))
            {
                return reader;
            }

            throw new CompactSerializerException(
                "Cannot deserialize property '{0}.{1}'. Cannot deserialize values of type '{2}'.",
                property.DeclaringType.Name, property.Name, property.PropertyType.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueReader GetValueReader(FieldInfo field)
        {
            ValueReader reader;

            if (TryGetValueReader(field.FieldType, out reader))
            {
                return reader;
            }

            throw new CompactSerializerException(
                "Cannot deserialize field '{0}.{1}'. Cannot deserialize values of type '{2}'.",
                field.DeclaringType.Name, field.Name, field.FieldType.FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool TryGetValueReader(Type type, out ValueReader reader)
        {
            var key = GetValueReaderWriterKey(type);
            return _s_valueReaderByType.TryGetValue(key, out reader);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsPrimitiveValueType(Type type)
        {
            if (type.IsValueType)
            {
                return (type.IsPrimitive || type.IsEnum);
            }
            else
            {
                return (type != typeof(string));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type GetValueReaderWriterKey(Type valueType)
        {
            if (valueType.IsEnum)
            {
                return typeof(Enum);
            }

            if (valueType == typeof(string))
            {
                return valueType;
            }

            if (!valueType.IsValueType)
            {
                return typeof(object);
            }

            if (!valueType.IsPrimitive && !_s_valueReaderByType.ContainsKey(valueType))
            {
                return typeof(ValueType);
            }

            return valueType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int RegisterDynamicMethod(Delegate methodDelegate)
        {
            lock (_s_dynamicMethodsSyncRoot)
            {
                var newList = new List<Delegate>(_s_dynamicMethods);
                newList.Add(methodDelegate);
                _s_dynamicMethods = newList;
                return newList.Count - 1;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly object _s_dynamicMethodsSyncRoot = new object();
        private static List<Delegate> _s_dynamicMethods = new List<Delegate>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, ValueWriter> _s_valueWriterByType = new Dictionary<Type, ValueWriter>() {
            {
                typeof(object),
                (type, w, context, value) =>
                    context.Void<Type, Object>(
                        x => x.WriteObject, 
                        w.Const(type), 
                        value.CastTo<object>()
                    )
            },
            {
                typeof(ValueType),
                (type, w, context, value) => {
                    using (TT.CreateScope<TT.TStruct>(type))
                    {
                        context.Void<TT.TStruct>(
                            x => x.WriteStruct<TT.TStruct>, 
                            value.CastTo<TT.TStruct>()
                        );
                    }
                }
            },
            {
                typeof(Enum),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, value.CastTo<int>())
            },
            {
                typeof(string),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.WriteStringOrNull, value.CastTo<string>())
            },
            {
                typeof(bool),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write, value.CastTo<bool>())
            },
            {
                typeof(int),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, value.CastTo<int>())
            },
            {
                typeof(long),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitLong, value.CastTo<long>())
            },
            {
                typeof(TimeSpan),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.WriteTimeSpan, value.CastTo<TimeSpan>())
            },
            {
                typeof(DateTime),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.WriteDateTime, value.CastTo<DateTime>())
            },
            {
                typeof(Guid),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.WriteGuid, value.CastTo<Guid>())
            },
            {
                typeof(float),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write, value.CastTo<float>())
            },
            {
                typeof(double),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write, value.CastTo<double>())
            },
            {
                typeof(decimal),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write, value.CastTo<decimal>())
            },
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, ValueReader> _s_valueReaderByType = new Dictionary<Type, ValueReader>() {
            {
                typeof(object),
                (type, w, context, assignTo) =>
                    assignTo.Assign(
                        context.Func<Type, object>(x => x.ReadObject, w.Const(type)).CastTo<TT.TItem>()
                    )
            },
            {
                typeof(ValueType),
                (type, w, context, assignTo) => {
                    using (TT.CreateScope<TT.TStruct>(type))
                    {
                        assignTo.Assign(context.Func<TT.TStruct>(x => x.ReadStruct<TT.TStruct>).CastTo<TT.TItem>());
                    }
                }
            },
            {
                typeof(Enum),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt).CastTo<TT.TItem>())
            },
            {
                typeof(string),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<string>(x => x.ReadStringOrNull).CastTo<TT.TItem>())
            },
            {
                typeof(bool),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<bool>(x => x.ReadBoolean).CastTo<TT.TItem>())
            },
            {
                typeof(int),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt).CastTo<TT.TItem>())
            },
            {
                typeof(long),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<long>(x => x.Read7BitLong).CastTo<TT.TItem>())
            },
            {
                typeof(TimeSpan),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<TimeSpan>(x => x.ReadTimeSpan).CastTo<TT.TItem>())
            },
            {
                typeof(DateTime),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<DateTime>(x => x.ReadDateTime).CastTo<TT.TItem>())
            },
            {
                typeof(Guid),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<Guid>(x => x.ReadGuid).CastTo<TT.TItem>())
            },
            {
                typeof(float),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<float>(x => x.ReadSingle).CastTo<TT.TItem>())
            },
            {
                typeof(double),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<double>(x => x.ReadDouble).CastTo<TT.TItem>())
            },
            {
                typeof(decimal),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<decimal>(x => x.ReadDecimal).CastTo<TT.TItem>())
            },
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void TypeReader(CompactDeserializationContext context, object obj);
        public delegate void TypeWriter(CompactSerializationContext context, object obj);
        public delegate object TypeCreator(CompactDeserializationContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void StructTypeReader<T>(CompactDeserializationContext context, ref T obj) where T : struct;
        public delegate void StructTypeWriter<T>(CompactSerializationContext context, ref T obj) where T : struct;
        public delegate T StructTypeCreator<T>(CompactDeserializationContext context) where T : struct;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void ValueReader(
            Type type,
            MethodWriterBase method,
            Argument<CompactDeserializationContext> context,
            MutableOperand<TT.TItem> assignTo);

        public delegate void ValueWriter(
            Type type,
            MethodWriterBase method,
            Argument<CompactSerializationContext> context,
            IOperand<TT.TItem> value);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITypeSerializer
        {
            object CreateObject(CompactDeserializationContext context);
            void ReadObject(CompactDeserializationContext context, object obj);
            void WriteObject(CompactSerializationContext context, object obj);
            Type ForType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITypeSerializer<T>
        {
            T CreateObject(CompactDeserializationContext context);
            void ReadObject(CompactDeserializationContext context, ref T obj);
            void WriteObject(CompactSerializationContext context, ref T obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ReferenceTypeSerializerConvention : ImplementationConvention
        {
            private readonly CompactSerializerFactory _factory;
            private readonly Type _forType;
            private readonly bool _isCollectionType;
            private readonly Type _collectionItemType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ReferenceTypeSerializerConvention(CompactSerializerFactory factory, Type forType)
                : base(Will.ImplementBaseClass)
            {
                _factory = factory;
                _forType = forType;
                _isCollectionType = forType.IsCollectionType(out _collectionItemType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override bool ShouldApply(ObjectFactoryContext context)
            {
                return !_forType.IsValueType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                using (TT.CreateScope<TT.TImpl, TT.TItem>(_forType, _collectionItemType ?? typeof(object)))
                {
                    writer.DefaultConstructor();
                    writer.ImplementInterface<ITypeSerializer>()
                        .Method<CompactDeserializationContext, object>(x => x.ReadObject).Implement(ImplementRead)
                        .Method<CompactSerializationContext, object>(x => x.WriteObject).Implement(ImplementWrite)
                        .Method<CompactDeserializationContext, object>(x => x.CreateObject).Implement(ImplementCreate)
                        .Property(x => x.ForType).Implement(p => p.Get(w => w.Return(w.Const(_forType))));
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementRead(VoidMethodWriter w, Argument<CompactDeserializationContext> context, Argument<object> obj)
            {
                if (_forType.IsArray)
                {
                    ImplementReadArray(w, context, obj);
                }
                else if (_isCollectionType)
                {
                    ImpementReadCollection(w, context, obj);
                }
                else
                {
                    ImplementReadPlainObject(w, context, obj);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWrite(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                if (_forType.IsArray)
                {
                    ImplementWriteArray(w, context, obj);
                }
                else if (_isCollectionType)
                {
                    if (_forType.IsGenericIListType())
                    {
                        ImplementWriteList(w, context, obj);
                    }
                    else
                    {
                        ImplementWriteCollection(w, context, obj);
                    }
                }
                else
                {
                    ImplementWritePlainObject(w, context, obj);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementReadArray(VoidMethodWriter w, Argument<CompactDeserializationContext> context, Argument<object> obj)
            {
                var itemReader = GetValueReader(_collectionItemType);

                using (TT.CreateScope<TT.TItem, TT.TStruct>(_collectionItemType, _collectionItemType))
                {
                    var typedArray = w.Local<TT.TItem[]>(initialValue: obj.CastTo<TT.TItem[]>());
                    var length = w.Local<int>(initialValue: typedArray.Length());
                    
                    w.For(from: 0, to: length).Do((loop, index) => {
                        itemReader(_collectionItemType, w, context, assignTo: typedArray.ElementAt(index));
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImpementReadCollection(VoidMethodWriter w, Argument<CompactDeserializationContext> context, Argument<object> obj)
            {
                var itemReader = GetValueReader(_collectionItemType);

                using (TT.CreateScope<TT.TItem, TT.TStruct>(_collectionItemType, _collectionItemType))
                {
                    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TItem>>());
                    Local<int> count = w.Local<int>();

                    if (_forType.IsGenericIListType())
                    {
                        count.Assign(typedCollection.CastTo<List<TT.TItem>>().Prop(x => x.Capacity));
                    }
                    else
                    {
                        count.Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt));
                    }

                    var item = w.Local<TT.TItem>();

                    w.For(from: 0, to: count).Do((loop, index) => {
                        itemReader(_collectionItemType, w, context, assignTo: item);
                        typedCollection.Void(x => x.Add, item);
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementReadPlainObject(VoidMethodWriter w, Argument<CompactDeserializationContext> context, Argument<object> obj)
            {
                using (TT.CreateScope<TT.TImpl>(_forType))
                {
                    var typedObj = w.Local<TT.TImpl>(initialValue: obj.CastTo<TT.TImpl>());
                    TypeMemberCache.Of(_forType).SelectAllProperties(@where: IsSerializableProperty).ForEach(p => {
                        using (TT.CreateScope<TT.TItem, TT.TStruct>(p.PropertyType, p.PropertyType))
                        {
                            var valueReader = GetValueReader(p);
                            valueReader(p.PropertyType, w, context, assignTo: typedObj.Prop<TT.TItem>(p));
                        }
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWriteArray(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var itemWriter = GetValueWriter(_collectionItemType);

                using (TT.CreateScope<TT.TItem, TT.TStruct>(_collectionItemType, _collectionItemType))
                {
                    var typedArray = w.Local<TT.TItem[]>(initialValue: obj.CastTo<TT.TItem[]>());
                    var length = w.Local<int>(initialValue: typedArray.Length());

                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, length);

                    w.For(from: 0, to: length).Do((loop, index) => {
                        itemWriter(_collectionItemType, w, context, value: typedArray.ElementAt(index));
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWriteList(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var itemWriter = GetValueWriter(_collectionItemType);

                using (TT.CreateScope<TT.TItem, TT.TStruct>(_collectionItemType, _collectionItemType))
                {
                    var typedList = w.Local(initialValue: obj.CastTo<IList<TT.TItem>>());
                    var count = w.Local<int>(initialValue: typedList.Count());

                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, count);

                    w.For(from: 0, to: count).Do((loop, index) => {
                        itemWriter(_collectionItemType, w, context, value: typedList.Item<int, TT.TItem>(index));
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWriteCollection(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var itemWriter = GetValueWriter(_collectionItemType);

                using (TT.CreateScope<TT.TItem, TT.TStruct>(_collectionItemType, _collectionItemType))
                {
                    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TItem>>());
                    var count = w.Local<int>(initialValue: typedCollection.Count());

                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, count);

                    w.ForeachElementIn(typedCollection).Do((loop, item) => {
                        itemWriter(_collectionItemType, w, context, value: item);
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWritePlainObject(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var typedObj = w.Local<TT.TImpl>(initialValue: obj.CastTo<TT.TImpl>());
                TypeMemberCache.Of(_forType).SelectAllProperties(@where: IsSerializableProperty).ForEach(p => {
                    using (TT.CreateScope<TT.TItem, TT.TStruct>(p.PropertyType, p.PropertyType))
                    {
                        var valueWriter = GetValueWriter(p);
                        valueWriter(p.PropertyType, w, context, value: typedObj.Prop<TT.TItem>(p));
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementCreate(FunctionMethodWriter<object> w, Argument<CompactDeserializationContext> context)
            {
                if (_forType.IsArray)
                {
                    ImplementCreateArray(w, context);
                }
                else if (_isCollectionType && _forType.IsGenericIListType())
                {
                    ImplementCreateList(w, context);
                }
                else
                {
                    ImplementCreatePlainObject(w, context);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementCreateArray(FunctionMethodWriter<object> w, Argument<CompactDeserializationContext> context)
            {
                using (TT.CreateScope<TT.TItem>(_collectionItemType))
                {
                    w.Return(w.NewArray<TT.TItem>(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt)));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementCreateList(FunctionMethodWriter<object> w, Argument<CompactDeserializationContext> context)
            {
                using (TT.CreateScope<TT.TItem>(_collectionItemType))
                {
                    w.Return(w.New<List<TT.TItem>>(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt)));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementCreatePlainObject(FunctionMethodWriter<object> w, Argument<CompactDeserializationContext> context)
            {
                var constructor = _forType.GetConstructors().OrderBy(c => c.GetParameters().Length).FirstOrDefault();

                //TODO: consider using FormatterServices.GetSafeUninitializedObject
                if (constructor == null)
                {
                    throw new CompactSerializerException("Type '{0}' cannot be deserialized as it has no public constructors.", _forType.FullName);
                }

                var parameters = constructor.GetParameters();
                var parameterLocals = new IOperand[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    using (TT.CreateScope<TT.TArgument>(parameters[i].ParameterType))
                    {
                        parameterLocals[i] = w.Local(initialValue: Static.Func(
                            ResolutionExtensions.Resolve<TT.TArgument>, 
                            context.Prop(x => x.Components)));
                    }
                }

                w.Return(w.New<TT.TImpl>(parameterLocals).CastTo<object>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StructTypeSerializer<T> : ITypeSerializer<T>, ITypeSerializer
            where T : struct
        {
            private readonly StructTypeReader<T> _reader;
            private readonly StructTypeWriter<T> _writer;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StructTypeSerializer(Delegate reader, Delegate writer)
            {
                _reader = (StructTypeReader<T>)reader;
                _writer = (StructTypeWriter<T>)writer;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T CreateObject(CompactDeserializationContext context)
            {
                return (T)FormatterServices.GetSafeUninitializedObject(typeof(T));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ReadObject(CompactDeserializationContext context, ref T obj)
            {
                _reader(context, ref obj);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WriteObject(CompactSerializationContext context, ref T obj)
            {
                _writer(context, ref obj);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object ITypeSerializer.CreateObject(CompactDeserializationContext context)
            {
                return CreateObject(context);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void ITypeSerializer.ReadObject(CompactDeserializationContext context, object obj)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void ITypeSerializer.WriteObject(CompactSerializationContext context, object obj)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type ITypeSerializer.ForType
            {
                get { return typeof(T); }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StructTypeSerializerFactory
        {
            private readonly CompactSerializerFactory _owner;
            private readonly Type _forType;
            private readonly FieldInfo[] _fields;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StructTypeSerializerFactory(CompactSerializerFactory owner, Type forType)
            {
                _owner = owner;
                _forType = forType;
                _fields = _forType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeSerializer CompileStructTypeSerializer()
            {
                var serializerConstructedType = typeof(StructTypeSerializer<>).MakeGenericType(_forType);
                var reader = ImplementReadStruct();
                var writer = ImplementWriteStruct();
                return (ITypeSerializer)Activator.CreateInstance(serializerConstructedType, reader, writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Delegate ImplementReadStruct()
            {
                using (TT.CreateScope<TT.TStruct>(_forType))
                {
                    var dynamicMethodDelegate = _owner.Compiler
                        .ForTemplatedDelegate<StructTypeReader<TT.TStruct>>()
                        .CompileStaticVoidMethod<CompactDeserializationContext, TT.TStruct>("ReadStruct_" + _forType.FriendlyName(),
                            (w, context, value) => {
                                foreach (var fieldInfo in _fields)
                                {
                                    using (TT.CreateScope<TT.TStruct, TT.TItem>(_forType, fieldInfo.FieldType))
                                    {
                                        var valueReader = GetValueReader(fieldInfo);
                                        valueReader(fieldInfo.FieldType, w, context, assignTo: value.Field<TT.TItem>(fieldInfo));
                                    }
                                }
                            });

                    return dynamicMethodDelegate;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Delegate ImplementWriteStruct()
            {
                using (TT.CreateScope<TT.TStruct>(_forType))
                {
                    var dynamicMethodDelegate = _owner.Compiler
                        .ForTemplatedDelegate<StructTypeWriter<TT.TStruct>>()
                        .CompileStaticVoidMethod<CompactSerializationContext, TT.TStruct>("WriteStruct_" + _forType.FriendlyName(),
                            (w, context, value) => {
                                foreach (var fieldInfo in _fields)
                                {
                                    using (TT.CreateScope<TT.TStruct, TT.TItem>(_forType, fieldInfo.FieldType))
                                    {
                                        var valueWriter = GetValueWriter(fieldInfo);
                                        valueWriter(fieldInfo.FieldType, w, context, value: value.Field<TT.TItem>(fieldInfo));
                                    }
                                }
                            });

                    return dynamicMethodDelegate;
                }
            }
        }
    }
}

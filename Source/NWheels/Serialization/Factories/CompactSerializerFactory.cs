using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Extensions;
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

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var forType = context.TypeKey.PrimaryInterface;

            return new IObjectFactoryConvention[] {
                new TypeSerializerConvention(this, forType), 
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ITypeSerializer CreateTypeSerializer(Type type)
        {
            return GetOrBuildType(new TypeKey(primaryInterface: type)).CreateInstance<ITypeSerializer>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsSerializableProperty(PropertyInfo property)
        {
            return (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0);
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

            return valueType;
        }

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
                typeof(string),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.WriteStringOrNull, value.CastTo<string>())
            },
            {
                typeof(Enum),
                (type, w, context, value) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, value.CastTo<int>())
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
                        context.Func<Type, object>(x => x.ReadObject, w.Const(type)).CastTo<TT.TValue>()
                    )
            },
            {
                typeof(string),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<string>(x => x.ReadStringOrNull).CastTo<TT.TValue>())
            },
            {
                typeof(Enum),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt).CastTo<TT.TValue>())
            },
            {
                typeof(bool),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<bool>(x => x.ReadBoolean).CastTo<TT.TValue>())
            },
            {
                typeof(int),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt).CastTo<TT.TValue>())
            },
            {
                typeof(long),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<long>(x => x.Read7BitLong).CastTo<TT.TValue>())
            },
            {
                typeof(TimeSpan),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<TimeSpan>(x => x.ReadTimeSpan).CastTo<TT.TValue>())
            },
            {
                typeof(DateTime),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<DateTime>(x => x.ReadDateTime).CastTo<TT.TValue>())
            },
            {
                typeof(Guid),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<Guid>(x => x.ReadGuid).CastTo<TT.TValue>())
            },
            {
                typeof(float),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<float>(x => x.ReadSingle).CastTo<TT.TValue>())
            },
            {
                typeof(double),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<double>(x => x.ReadDouble).CastTo<TT.TValue>())
            },
            {
                typeof(decimal),
                (type, w, context, assignTo) =>
                    assignTo.Assign(context.Prop(x => x.Input).Func<decimal>(x => x.ReadDecimal).CastTo<TT.TValue>())
            },
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void TypeReader(CompactDeserializationContext context, object obj);
        public delegate void TypeWriter(CompactSerializationContext context, object obj);
        public delegate object TypeCreator(CompactDeserializationContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void ValueReader(
            Type type,
            MethodWriterBase method,
            Argument<CompactDeserializationContext> context,
            MutableOperand<TT.TValue> assignTo);

        public delegate void ValueWriter(
            Type type,
            MethodWriterBase method,
            Argument<CompactSerializationContext> context,
            IOperand<TT.TValue> value);

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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class RuntimeHelpers
        {
            public static void WriteCollection<T>(
                CompactSerializationContext context, 
                ICollection<T> collection, 
                Action<CompactSerializationContext, T> onWriteItem)
            {
                context.Output.WriteCollection<T>(collection, (bw, item, state) => onWriteItem(context, item), null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void ReadCollection<T>(
                CompactDeserializationContext context,
                ICollection<T> collection,
                Func<CompactDeserializationContext, T> onReadItem)
            {
                context.Input.ReadCollection(collection, (br, state) => onReadItem(context), null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static T[] ReadArray<T>(
                CompactDeserializationContext context,
                Func<CompactDeserializationContext, T> onReadItem)
            {
                return context.Input.ReadArray<T>((br, state) => onReadItem(context), null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TypeSerializerConvention : ImplementationConvention
        {
            private readonly CompactSerializerFactory _factory;
            private readonly Type _forType;
            private readonly bool _isCollectionType;
            private readonly Type _collectionItemType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeSerializerConvention(CompactSerializerFactory factory, Type forType)
                : base(Will.ImplementBaseClass)
            {
                _factory = factory;
                _forType = forType;
                _isCollectionType = forType.IsCollectionType(out _collectionItemType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                using (TT.CreateScope<TT.TImpl, TT.TValue>(_forType, _collectionItemType ?? typeof(object)))
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

                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    var typedArray = w.Local<TT.TValue[]>(initialValue: obj.CastTo<TT.TValue[]>());
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

                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                    Local<int> count = w.Local<int>();

                    if (_forType.IsGenericIListType())
                    {
                        count.Assign(typedCollection.CastTo<List<TT.TValue>>().Prop(x => x.Capacity));
                    }
                    else
                    {
                        count.Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt));
                    }

                    var item = w.Local<TT.TValue>();

                    w.For(from: 0, to: count).Do((loop, index) => {
                        itemReader(_collectionItemType, w, context, assignTo: item);
                        typedCollection.Void(x => x.Add, item);
                    });
                }

                //Type itemType;
                //_forType.IsCollectionType(out itemType);

                //using (TT.CreateScope<TT.TValue>(itemType))
                //{
                //    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                //    Static.GenericVoid((x, y, z) => RuntimeHelpers.ReadCollection(x, y, z),
                //        context,
                //        typedCollection,
                //        w.Delegate<CompactDeserializationContext, TT.TValue>((ww, ctx) => {
                //            var itemReader = GetValueReader(itemType);
                //            var item = ww.Local<TT.TValue>();
                //            itemReader(itemType, ww, context, item);
                //            ww.Return(item);
                //        })
                //    );
                //}
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementReadPlainObject(VoidMethodWriter w, Argument<CompactDeserializationContext> context, Argument<object> obj)
            {
                using (TT.CreateScope<TT.TImpl>(_forType))
                {
                    var typedObj = w.Local<TT.TImpl>(initialValue: obj.CastTo<TT.TImpl>());
                    TypeMemberCache.Of(_forType).SelectAllProperties(@where: IsSerializableProperty).ForEach(p => {
                        using (TT.CreateScope<TT.TValue>(p.PropertyType))
                        {
                            var valueReader = GetValueReader(p);
                            valueReader(p.PropertyType, w, context, assignTo: typedObj.Prop<TT.TValue>(p));
                        }
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWriteArray(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var itemWriter = GetValueWriter(_collectionItemType);

                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    var typedArray = w.Local<TT.TValue[]>(initialValue: obj.CastTo<TT.TValue[]>());
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

                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    var typedList = w.Local(initialValue: obj.CastTo<IList<TT.TValue>>());
                    var count = w.Local<int>(initialValue: typedList.Count());

                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, count);

                    w.For(from: 0, to: count).Do((loop, index) => {
                        itemWriter(_collectionItemType, w, context, value: typedList.Item<int, TT.TValue>(index));
                    });
                }

                //Type itemType;
                //_forType.IsCollectionType(out itemType);

                //using (TT.CreateScope<TT.TValue>(itemType))
                //{
                //    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                //    Static.GenericVoid((x, y, z) => RuntimeHelpers.WriteCollection(x, y, z),
                //        context,
                //        typedCollection,
                //        w.Delegate<CompactSerializationContext, TT.TValue>((ww, ctx, item) => {
                //            var itemLocal = ww.Local<TT.TValue>(initialValue: item);
                //            var itemWriter = GetValueWriter(itemType);
                //            itemWriter(itemType, ww, context, value: itemLocal);
                //        })
                //    );
                //}
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWriteCollection(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var itemWriter = GetValueWriter(_collectionItemType);

                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                    var count = w.Local<int>(initialValue: typedCollection.Count());

                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, count);

                    w.ForeachElementIn(typedCollection).Do((loop, item) => {
                        itemWriter(_collectionItemType, w, context, value: item);
                    });
                }

                //Type itemType;
                //_forType.IsCollectionType(out itemType);

                //using (TT.CreateScope<TT.TValue>(itemType))
                //{
                //    var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                //    Static.GenericVoid((x, y, z) => RuntimeHelpers.WriteCollection(x, y, z),
                //        context,
                //        typedCollection,
                //        w.Delegate<CompactSerializationContext, TT.TValue>((ww, ctx, item) => {
                //            var itemLocal = ww.Local<TT.TValue>(initialValue: item);
                //            var itemWriter = GetValueWriter(itemType);
                //            itemWriter(itemType, ww, context, value: itemLocal);
                //        })
                //    );
                //}
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementWritePlainObject(VoidMethodWriter w, Argument<CompactSerializationContext> context, Argument<object> obj)
            {
                var typedObj = w.Local<TT.TImpl>(initialValue: obj.CastTo<TT.TImpl>());
                TypeMemberCache.Of(_forType).SelectAllProperties(@where: IsSerializableProperty).ForEach(p => {
                    using (TT.CreateScope<TT.TValue>(p.PropertyType))
                    {
                        var valueWriter = GetValueWriter(p);
                        valueWriter(p.PropertyType, w, context, value: typedObj.Prop<TT.TValue>(p));
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
                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    w.Return(w.NewArray<TT.TValue>(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt)));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementCreateList(FunctionMethodWriter<object> w, Argument<CompactDeserializationContext> context)
            {
                using (TT.CreateScope<TT.TValue>(_collectionItemType))
                {
                    w.Return(w.New<List<TT.TValue>>(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt)));
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
    }
}

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
using NWheels.TypeModel.Serialization;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Factories
{
    public class CompactTypeSerializerFactory
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly DynamicMethodCompiler _compiler;
        private readonly ConcurrentDictionary<Type, TypeReader> _readerByType;
        private readonly ConcurrentDictionary<Type, TypeWriter> _writerByType;
        private readonly ConcurrentDictionary<Type, TypeCreator> _defaultCreatorByType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactTypeSerializerFactory(ITypeMetadataCache metadataCache, DynamicModule dynamicModule)
        {
            _metadataCache = metadataCache;
            _compiler = new DynamicMethodCompiler(dynamicModule);
            _readerByType = new ConcurrentDictionary<Type, TypeReader>();
            _writerByType = new ConcurrentDictionary<Type, TypeWriter>();
            _defaultCreatorByType = new ConcurrentDictionary<Type, TypeCreator>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeReader GetTypeReader(Type type)
        {
            return _readerByType.GetOrAdd(type, CompileTypeReader);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeWriter GetTypeWriter(Type type)
        {
            return _writerByType.GetOrAdd(type, CompileTypeWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeCreator GetDefaultCreator(Type type)
        {
            return _defaultCreatorByType.GetOrAdd(type, k => CompileDefaultCreator(k));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void TypeReader(CompactDeserializationContext context, object obj);
        public delegate void TypeWriter(CompactSerializationContext context, object obj);
        public delegate object TypeCreator(IComponentContext components);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void PropertyReader(
            MethodWriterBase method,
            Argument<CompactDeserializationContext> context,
            Local<TT.TImpl> target,
            PropertyInfo prop);

        public delegate void PropertyWriter(
            MethodWriterBase method,
            Argument<CompactSerializationContext> context,
            Local<TT.TImpl> target,
            PropertyInfo prop);

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

        private TypeReader CompileTypeReader(Type type)
        {
            if (type.IsArray)
            {
                return CompileArrayReader(type);
            }
            
            if (type.IsCollectionType())
            {
                return CompileCollectionReader(type);
            }

            using (TT.CreateScope<TT.TImpl>(type))
            {
                var methodDelegate = _compiler.CompileStaticVoidMethod<CompactDeserializationContext, object>(
                    type.FullName + "_CompactSerializerReader",
                    (w, context, obj) => {
                        var typedObj = w.Local<TT.TImpl>(initialValue: obj.CastTo<TT.TImpl>());
                        TypeMemberCache.Of(type).SelectAllProperties(where: IsSerializableProperty).ForEach(p => {
                            var propertyReader = GetPropertyReader(p);
                            propertyReader(w, context, typedObj, p);
                        });
                    }
                );
                return new TypeReader(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeWriter CompileTypeWriter(Type type)
        {
            if (type.IsCollectionType())
            {
                return CompileCollectionWriter(type);
            }

            using ( TT.CreateScope<TT.TImpl>(type) )
            { 
                var methodDelegate = _compiler.CompileStaticVoidMethod<CompactSerializationContext, object>(
                    type.FullName + "_CompactSerializerReader",
                    (w, context, obj) => {
                        var typedObj = w.Local<TT.TImpl>(initialValue: obj.CastTo<TT.TImpl>());
                        TypeMemberCache.Of(type).SelectAllProperties(where: IsSerializableProperty).ForEach(p => {
                            var propertyWriter = GetPropertyWriter(p);
                            propertyWriter(w, context, typedObj, p);
                        });
                    }
                );
                return new TypeWriter(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeWriter CompileCollectionWriter(Type type)
        {
            Type itemType;
            type.IsCollectionType(out itemType);

            using (TT.CreateScope<TT.TImpl, TT.TValue>(type, itemType))
            {
                var methodDelegate = _compiler.CompileStaticVoidMethod<CompactSerializationContext, object>(
                    type.FriendlyName() + "_CompactSerializerReader",
                    (w, context, obj) => {
                        var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                        Static.GenericVoid((x, y, z) => RuntimeHelpers.WriteCollection(x, y, z),
                            context,
                            typedCollection,
                            w.Delegate<CompactSerializationContext, TT.TValue>((ww, ctx, item) => {
                                var itemWriter = GetValueWriter(itemType);
                                itemWriter(itemType, ww, context, item);
                            })
                        );
                    }
                );
                return new TypeWriter(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeReader CompileCollectionReader(Type type)
        {
            Type itemType;
            type.IsCollectionType(out itemType);

            using (TT.CreateScope<TT.TImpl, TT.TValue>(type, itemType))
            {
                var methodDelegate = _compiler.CompileStaticVoidMethod<CompactDeserializationContext, object>(
                    type.FriendlyName() + "_CompactSerializerReader",
                    (w, context, obj) => {
                        var typedCollection = w.Local(initialValue: obj.CastTo<ICollection<TT.TValue>>());
                        Static.GenericVoid((x, y, z) => RuntimeHelpers.ReadCollection(x, y, z),
                            context,
                            typedCollection,
                            w.Delegate<CompactDeserializationContext, TT.TValue>((ww, ctx) => {
                                var itemReader = GetValueReader(itemType);
                                var item = ww.Local<TT.TValue>();
                                itemReader(itemType, ww, context, item);
                                ww.Return(item);
                            })
                        );
                    }
                );
                return new TypeReader(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeReader CompileArrayReader(Type type)
        {
            Type itemType;
            type.IsCollectionType(out itemType);

            using (TT.CreateScope<TT.TImpl, TT.TValue>(type, itemType))
            {
                var methodDelegate = _compiler.CompileStaticVoidMethod<CompactDeserializationContext, object>(
                    type.FriendlyName() + "_CompactSerializerReader",
                    (w, context, obj) => {
                        Static.GenericVoid((x, y) => RuntimeHelpers.ReadArray(x, y),
                            context,
                            w.Delegate<CompactDeserializationContext, TT.TValue>((ww, ctx) => {
                                var itemReader = GetValueReader(itemType);
                                var item = ww.Local<TT.TValue>();
                                itemReader(itemType, ww, context, item);
                                ww.Return(item);
                            })
                        );
                    }
                );
                return new TypeReader(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsSerializableProperty(PropertyInfo property)
        {
            return (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeCreator CompileDefaultCreator(Type type)
        {
            var constructor = type.GetConstructors().OrderBy(c => c.GetParameters().Length).FirstOrDefault();

            //TODO: consider using FormatterServices.GetSafeUninitializedObject
            if (constructor == null)
            {
                throw new CompactObjectSerializerException("Type '{0}' cannot be deserialized as it has no public constructors.", type.FullName);
            }

            var parameters = constructor.GetParameters();

            var compiledFunc = _compiler.CompileStaticFunction<IComponentContext, object>(
                type.FullName + "_DefaultCreator",
                (w, components) => {
                    using (TT.CreateScope<TT.TImpl>(type))
                    {
                        var parameterLocals = new IOperand[parameters.Length];

                        for (int i = 0 ; i < parameters.Length ; i++)
                        {
                            using (TT.CreateScope<TT.TArgument>(parameters[i].ParameterType))
                            {
                                parameterLocals[i] = w.Local(initialValue: Static.Func(ResolutionExtensions.Resolve<TT.TArgument>, components));
                            }
                        }

                        w.Return(w.New<TT.TImpl>(parameterLocals).CastTo<object>());
                    }
                }
            );

            return new TypeCreator(compiledFunc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static PropertyWriter GetPropertyWriter(PropertyInfo property)
        {
            var key = GetValueReaderWriterKey(property.PropertyType);
            PropertyWriter writer;

            if (_s_propertyWriterByType.TryGetValue(key, out writer))
            {
                return writer;
            }

            throw new CompactObjectSerializerException(
                "Value of type '{0}' cannot be serialized ({1}.{2}).",
                property.PropertyType.FullName,
                property.DeclaringType.Name,
                property.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static PropertyReader GetPropertyReader(PropertyInfo property)
        {
            var key = GetValueReaderWriterKey(property.PropertyType);
            PropertyReader reader;

            if (_s_propertyReaderByType.TryGetValue(key, out reader))
            {
                return reader;
            }

            throw new CompactObjectSerializerException(
                "Value of type '{0}' cannot be deserialized ({1}.{2}).",
                property.PropertyType.FullName,
                property.DeclaringType.Name,
                property.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueWriter GetValueWriter(Type type)
        {
            var key = GetValueReaderWriterKey(type);
            ValueWriter writer;

            if (_s_valueWriterByType.TryGetValue(key, out writer))
            {
                return writer;
            }

            throw new CompactObjectSerializerException("Value of type '{0}' cannot be serialized.", type.FullName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ValueReader GetValueReader(Type type)
        {
            var key = GetValueReaderWriterKey(type);
            ValueReader reader;

            if (_s_valueReaderByType.TryGetValue(key, out reader))
            {
                return reader;
            }

            throw new CompactObjectSerializerException("Value of type '{0}' cannot be deserialized.", type.FullName);
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

        private static readonly Dictionary<Type, PropertyReader> _s_propertyReaderByType = new Dictionary<Type, PropertyReader>() {
            {
                typeof(object),
                (w, context, target, prop) =>
                    target.Prop<TT.TProperty>(prop).Assign(
                        context.Func<Type, object>(x => x.ReadObject, w.Const(prop.PropertyType)).CastTo<TT.TProperty>()
                    )
            },
            {
                typeof(string),
                (w, context, target, prop) =>
                    target.Prop<string>(prop).Assign(context.Prop(x => x.Input).Func<string>(x => x.ReadString))
            },
            {
                typeof(Enum),
                (w, context, target, prop) =>
                    target.Prop<TT.TProperty>(prop).Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt).CastTo<TT.TProperty>())
            },
            {
                typeof(bool),
                (w, context, target, prop) =>
                    target.Prop<bool>(prop).Assign(context.Prop(x => x.Input).Func<bool>(x => x.ReadBoolean))
            },
            {
                typeof(int),
                (w, context, target, prop) =>
                    target.Prop<int>(prop).Assign(context.Prop(x => x.Input).Func<int>(x => x.Read7BitInt))
            },
            {
                typeof(long),
                (w, context, target, prop) =>
                    target.Prop<long>(prop).Assign(context.Prop(x => x.Input).Func<long>(x => x.Read7BitLong))
            },
            {
                typeof(TimeSpan),
                (w, context, target, prop) =>
                    target.Prop<TimeSpan>(prop).Assign(context.Prop(x => x.Input).Func<TimeSpan>(x => x.ReadTimeSpan))
            },
            {
                typeof(DateTime),
                (w, context, target, prop) =>
                    target.Prop<DateTime>(prop).Assign(context.Prop(x => x.Input).Func<DateTime>(x => x.ReadDateTime))
            },
            {
                typeof(Guid),
                (w, context, target, prop) =>
                    target.Prop<Guid>(prop).Assign(context.Prop(x => x.Input).Func<Guid>(x => x.ReadGuid))
            },
            {
                typeof(float),
                (w, context, target, prop) =>
                    target.Prop<float>(prop).Assign(context.Prop(x => x.Input).Func<float>(x => x.ReadSingle))
            },
            {
                typeof(double),
                (w, context, target, prop) =>
                    target.Prop<double>(prop).Assign(context.Prop(x => x.Input).Func<double>(x => x.ReadDouble))
            },
            {
                typeof(decimal),
                (w, context, target, prop) =>
                    target.Prop<decimal>(prop).Assign(context.Prop(x => x.Input).Func<decimal>(x => x.ReadDecimal))
            },
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, PropertyWriter> _s_propertyWriterByType = new Dictionary<Type, PropertyWriter>() {
            {
                typeof(object),
                (w, context, target, prop) =>
                    context.Void<Type, Object>(
                        x => x.WriteObject, 
                        w.Const(prop.PropertyType), 
                        target.Prop<TT.TProperty>(prop).CastTo<object>()
                    )
            },
            {
                typeof(string),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write, target.Prop<string>(prop))
            },
            {
                typeof(Enum),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, target.Prop<TT.TProperty>(prop).CastTo<int>())
            },
            {
                typeof(bool),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write, target.Prop<bool>(prop))
            },
            {
                typeof(int),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitInt, target.Prop<int>(prop))
            },
            {
                typeof(long),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write7BitLong, target.Prop<long>(prop))
            },
            {
                typeof(TimeSpan),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.WriteTimeSpan, target.Prop<TimeSpan>(prop))
            },
            {
                typeof(DateTime),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.WriteDateTime, target.Prop<DateTime>(prop))
            },
            {
                typeof(Guid),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.WriteGuid, target.Prop<Guid>(prop))
            },
            {
                typeof(float),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write, target.Prop<float>(prop))
            },
            {
                typeof(double),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write, target.Prop<double>(prop))
            },
            {
                typeof(decimal),
                (w, context, target, prop) =>
                    context.Prop(x => x.Output).Void(x => x.Write, target.Prop<decimal>(prop))
            },
        };

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
                    context.Prop(x => x.Output).Void(x => x.Write, value.CastTo<string>())
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
                    assignTo.Assign(context.Prop(x => x.Input).Func<string>(x => x.ReadString).CastTo<TT.TValue>())
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
    }
}

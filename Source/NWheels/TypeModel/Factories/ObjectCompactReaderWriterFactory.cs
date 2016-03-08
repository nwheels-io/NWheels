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

        public delegate void Reader(CompactDeserializationContext context, object obj);
        public delegate void Writer(CompactSerializationContext context, object obj);
        public delegate object Creator(IComponentContext components);

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

        private Reader CompileReader(Type type)
        {
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
                return new Reader(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Writer CompileWriter(Type type)
        {
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
                return new Writer(methodDelegate);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsSerializableProperty(PropertyInfo property)
        {
            return (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Creator CompileDefaultCreator(Type type)
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

            return new Creator(compiledFunc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static PropertyWriter GetPropertyWriter(PropertyInfo property)
        {
            var key = GetPropertyReaderWriterKey(property);
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
            var key = GetPropertyReaderWriterKey(property);
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

        private static Type GetPropertyReaderWriterKey(PropertyInfo property)
        {
            var type = property.PropertyType;

            if (type.IsEnum)
            {
                return typeof(Enum);
            }

            if (type == typeof(string))
            {
                return type;
            }
            
            if (!type.IsValueType)
            {
                return typeof(object);
            }

            return type;
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
                    context.Prop(x => x.Output).Void(x =>  x.Write, target.Prop<string>(prop))
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
    }
}

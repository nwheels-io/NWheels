using System;
using System.Collections.Generic;
using System.Linq;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using TT = Hapil.TypeTemplate;

namespace NWheels.Logging.Core
{
    internal class NameValuePairLogNodeBuilder
    {
        private readonly bool _souldBuildActivity;
        private readonly List<Type> _types;
        private readonly List<IOperand<string>> _names;
        private readonly List<IOperand> _values;
        private readonly List<string> _formats;
        private readonly List<bool?> _isDetails;
        private readonly List<int?> _maxStringLengths;
        private readonly List<LogContentTypes?> _contentTypes;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNodeBuilder(bool shouldBuildActivity)
        {
            _souldBuildActivity = shouldBuildActivity;

            _types = new List<Type>(capacity: _s_logNodeGenericTypeByValueCount.Length);
            _names = new List<IOperand<string>>(capacity: _s_logNodeGenericTypeByValueCount.Length);
            _values = new List<IOperand>(capacity: _s_logNodeGenericTypeByValueCount.Length);
            _formats = new List<string>(capacity: _s_logNodeGenericTypeByValueCount.Length);
            _isDetails = new List<bool?>(capacity: _s_logNodeGenericTypeByValueCount.Length);
            _maxStringLengths = new List<int?>(capacity: _s_logNodeGenericTypeByValueCount.Length);
            _contentTypes = new List<LogContentTypes?>(capacity: _s_logNodeGenericTypeByValueCount.Length);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddNameValuePair(
            IOperand<string> name,
            Type type,
            IOperand value, 
            string format = null,
            bool? isDetail = null,
            int? maxStringLength = null,
            LogContentTypes? contentTypes = null)
        {
            if ( _types.Count >= _s_logNodeGenericTypeByValueCount.Length )
            {
                return;
            }

            _names.Add(name);
            _types.Add(TypeTemplate.Resolve(ByValType(type)));
            _values.Add(value);
            _formats.Add(format);
            _isDetails.Add(isDetail);
            _maxStringLengths.Add(maxStringLength);
            _contentTypes.Add(contentTypes);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetLogNodeType()
        {
            var openType = (
                _souldBuildActivity ? 
                _s_activityNodeGenericTypeByValueCount[_types.Count] : 
                _s_logNodeGenericTypeByValueCount[_types.Count]);

            if ( openType.IsGenericType && openType.IsGenericTypeDefinition )
            {
                return openType.MakeGenericType(_types.Select(ByValType).ToArray());
            }
            else
            {
                return openType;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IOperand<LogNode> GetNewLogNodeOperand(
            MethodWriterBase writer, 
            IOperand<string> messageId, 
            IOperand logLevelOperand = null, 
            IOperand exceptionOperand = null)
        {
            var constructorArguments = new List<IOperand>();
            constructorArguments.Add(messageId);

            if ( !_souldBuildActivity )
            {
                constructorArguments.Add(logLevelOperand);
                constructorArguments.Add(exceptionOperand);
            }

            for ( int i = 0 ; i < _types.Count ; i++ )
            {
                using ( TT.CreateScope<TT.TArgument>(_types[i]) )
                {
                    var pairLocal = writer.Local<LogNameValuePair<TT.TArgument>>();

                    pairLocal.Assign(writer.New<LogNameValuePair<TT.TArgument>>());
                    SetNameValuePairFields(pairLocal, i);

                    constructorArguments.Add(pairLocal);
                }
            }

            using ( TT.CreateScope<TT.TValue>(GetLogNodeType()) )
            {
                return writer.New<TT.TValue>(constructorArguments.ToArray()).CastTo<LogNode>();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public int NameValuePairCount
        {
            get { return _types.Count; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetNameValuePairFields(Local<LogNameValuePair<TypeTemplate.TArgument>> pairLocal, int index)
        {
            pairLocal.Field(x => x.Name).Assign(_names[index]);
            pairLocal.Field(x => x.Value).Assign(_values[index].CastTo<TT.TArgument>());

            if ( _formats[index] != null )
            {
                pairLocal.Field(x => x.Format).Assign(_formats[index]);
            }
            if ( _isDetails[index].HasValue )
            {
                pairLocal.Field(x => x.IsDetail).Assign(_isDetails[index].Value);
            }
            if ( _maxStringLengths[index].HasValue )
            {
                pairLocal.Field(x => x.MaxStringLength).Assign(_maxStringLengths[index].Value);
            }
            if ( _contentTypes[index].HasValue )
            {
                pairLocal.Field(x => x.ContentTypes).Assign(_contentTypes[index].Value);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private Type ByValType(Type type)
        {
            if ( type.IsByRef )
            {
                return type.GetElementType();
            }
            else
            {
                return type;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Type[] _s_activityNodeGenericTypeByValueCount = new Type[] {
            typeof(NameValuePairActivityLogNode),
            typeof(NameValuePairActivityLogNode<>),
            typeof(NameValuePairActivityLogNode<,>),
            typeof(NameValuePairActivityLogNode<,,>),
            typeof(NameValuePairActivityLogNode<,,,>),
            typeof(NameValuePairActivityLogNode<,,,,>),
            typeof(NameValuePairActivityLogNode<,,,,,>),
            typeof(NameValuePairActivityLogNode<,,,,,,>),
            typeof(NameValuePairActivityLogNode<,,,,,,,>)
        };

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Type[] _s_logNodeGenericTypeByValueCount = new Type[] {
            typeof(NameValuePairLogNode),
            typeof(NameValuePairLogNode<>),
            typeof(NameValuePairLogNode<,>),
            typeof(NameValuePairLogNode<,,>),
            typeof(NameValuePairLogNode<,,,>),
            typeof(NameValuePairLogNode<,,,,>),
            typeof(NameValuePairLogNode<,,,,,>),
            typeof(NameValuePairLogNode<,,,,,,>),
            typeof(NameValuePairLogNode<,,,,,,,>)
        };
    }
}
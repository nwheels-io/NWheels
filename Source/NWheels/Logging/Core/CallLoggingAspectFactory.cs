using System;
using System.Collections.Generic;
using System.Linq;
using Hapil;
using Hapil.Applied.Conventions;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;

namespace NWheels.Logging.Core
{
    public class CallLoggingAspectFactory : ConventionObjectFactory
    {
        public CallLoggingAspectFactory(DynamicModule module)
            : base(module, context => new IObjectFactoryConvention[] { new CallTargetConvention(), new AspectConvention() })
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string LoggingCallOutputsMessageId = "CallLoggingAspect.LoggingCallOutputs";
        public static readonly string CallOutputReturnValueName = "return";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AspectConvention : DecorationConvention
        {
            private readonly Dictionary<string, Field<string>> _staticStringFields;
            private TypeKey _typeKey;
            private DecoratingClassWriter _classWriter;
            private Field<IThreadLogAppender> _threadLogAppenderField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AspectConvention()
                : base(Will.DecorateClass | Will.DecorateMethods)
            {
                _staticStringFields = new Dictionary<string, Field<string>>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override bool ShouldApply(ObjectFactoryContext context)
            {
                _typeKey = context.TypeKey;
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
            {
                _classWriter = classWriter;
                _threadLogAppenderField = classWriter.DependencyField<IThreadLogAppender>("_threadLogAppender");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
            {
                var messageId = LogNode.PreserveMessageIdPrefix + _typeKey.PrimaryInterface.Name.TrimPrefix("I") + "." + member.Name;
                var inputLogNodeBuilder = new NameValuePairLogNodeBuilder(shouldBuildActivity: true);
                var outputLogNodeBuilder = new NameValuePairLogNodeBuilder(shouldBuildActivity: false);
                
                Local<ActivityLogNode> activityLocal = null;

                decorate()
                    .OnBefore(w => {
                        activityLocal = w.Local<ActivityLogNode>();
                    })
                    .OnInputArgument((w, arg) => {
                        inputLogNodeBuilder.AddNameValuePair(w.Const(arg.Name), arg.OperandType, arg, isDetail: true);
                    })
                    .OnInspectedInputArguments(w => {
                        activityLocal.Assign(inputLogNodeBuilder.GetNewLogNodeOperand(w, GetStaticStringOperand(messageId)).CastTo<ActivityLogNode>());
                        _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal);
                    })
                    .OnException<Exception>((w, exception) => {
                        activityLocal.CastTo<ILogActivity>().Void(x => x.Fail, exception);
                        w.Throw();
                    })
                    .OnOutputArgument((w, arg) => {
                        outputLogNodeBuilder.AddNameValuePair(w.Const(arg.Name), arg.OperandType, arg, isDetail: true);
                    })
                    .OnReturnValue((w, retVal) => {
                        outputLogNodeBuilder.AddNameValuePair(
                            Static.Prop(() => CallOutputReturnValueName), w.OwnerMethod.Signature.ReturnType, retVal, isDetail: true);
                    })
                    .OnSuccess(w => {
                        if ( outputLogNodeBuilder.NameValuePairCount > 0 )
                        {
                            var logNodeOperand = outputLogNodeBuilder.GetNewLogNodeOperand(
                                w, GetStaticStringOperand(LoggingCallOutputsMessageId), w.Const(LogLevel.Debug), w.Const<Exception>(null));
                            _threadLogAppenderField.Void(x => x.AppendLogNode, logNodeOperand);
                        }
                    })
                    .OnAfter(w => {
                        activityLocal.CastTo<ILogActivity>().Void(x => x.Dispose);     
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnFinalizeDecoration(ClassType classType, DecoratingClassWriter classWriter)
            {
                classWriter.ImplementBase<object>().StaticConstructor(cw => {
                    foreach ( var staticString in _staticStringFields )
                    {
                        staticString.Value.Assign(staticString.Key);
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IOperand<string> GetStaticStringOperand(string s)
            {
                Field<string> field;

                if ( !_staticStringFields.TryGetValue(s, out field) )
                {
                    field = _classWriter.StaticField<string>("_s_string_" + new string(s.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray()));
                    _staticStringFields.Add(s, field);
                }

                return field;
            }
        }
    }
}

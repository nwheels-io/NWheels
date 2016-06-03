using System;
using System.Collections.Generic;
using System.Linq;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.Hosting.Core;
using NWheels.Hosting.Factories;
using NWheels.Logging.Core;

namespace NWheels.Logging.Factories
{
    public class CallLoggingAspectConvention : DecorationConvention
    {
        public static readonly string LoggingCallOutputsMessageId = "CallLoggingAspect.LoggingCallOutputs";
        public static readonly string CallOutputReturnValueName = "return";

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly StaticStringsDecorator _staticStrings;
        private TypeKey _typeKey;
        private DecoratingClassWriter _classWriter;
        private Field<IThreadLogAppender> _threadLogAppenderField;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public CallLoggingAspectConvention(ComponentAspectFactory.ConventionContext aspectContext)
            : base(Will.DecorateClass | Will.DecorateMethods)
        {
            _staticStrings = aspectContext.StaticStrings;
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
                    activityLocal.Assign(inputLogNodeBuilder.GetNewLogNodeOperand(w, _staticStrings.GetStaticStringOperand(messageId)).CastTo<ActivityLogNode>());
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
                            w, _staticStrings.GetStaticStringOperand(LoggingCallOutputsMessageId), w.Const(LogLevel.Debug), w.Const<Exception>(null));
                        _threadLogAppenderField.Void(x => x.AppendLogNode, logNodeOperand);
                    }
                })
                .OnAfter(w => {
                    activityLocal.CastTo<ILogActivity>().Void(x => x.Dispose);     
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AspectProvider : IComponentAspectProvider
        {
            #region Implementation of IComponentAspectProvider

            public IObjectFactoryConvention GetAspectConvention(ComponentAspectFactory.ConventionContext context)
            {
                return new CallLoggingAspectConvention(context);
            }

            #endregion
        }
    }
}
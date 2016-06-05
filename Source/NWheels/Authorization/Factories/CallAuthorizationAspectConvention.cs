using System;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Threading;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Authorization.Core;
using NWheels.Conventions.Core;
using NWheels.Exceptions;
using NWheels.Hosting.Core;
using NWheels.Hosting.Factories;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Logging.Factories;

namespace NWheels.Authorization.Factories
{
    public class CallAuthorizationAspectConvention : DecorationConvention
    {
        public static readonly string LoggingCallOutputsMessageId = "CallLoggingAspect.LoggingCallOutputs";
        public static readonly string CallOutputReturnValueName = "return";

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly ComponentAspectFactory.ConventionContext _aspectContext;
        private readonly StaticStringsDecorator _staticStrings;
        private readonly AccessSecurityAttributeBase _classLevelAttribute;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public CallAuthorizationAspectConvention(ComponentAspectFactory.ConventionContext aspectContext)
            : base(Will.DecorateClass | Will.DecorateMethods)
        {
            _aspectContext = aspectContext;
            _staticStrings = aspectContext.StaticStrings;
            _classLevelAttribute = _aspectContext.ComponentType.GetCustomAttribute<AccessSecurityAttributeBase>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
        {
            var methodLevelAttribute = (member.MethodDeclaration != null ? member.MethodDeclaration.GetCustomAttribute<AccessSecurityAttributeBase>() : null);
            var effectiveAttribute = (methodLevelAttribute ?? _classLevelAttribute);

            decorate().OnBefore(w => {
                if (effectiveAttribute != null)
                {
                    effectiveAttribute.WriteSecurityCheck(w, _staticStrings);
                }
                else
                {
                    Static.Void(SecurityCheck.RequireAuthentication);
                }
            });

            //var messageId = LogNode.PreserveMessageIdPrefix + _typeKey.PrimaryInterface.Name.TrimPrefix("I") + "." + member.Name;
            //var inputLogNodeBuilder = new NameValuePairLogNodeBuilder(shouldBuildActivity: true);
            //var outputLogNodeBuilder = new NameValuePairLogNodeBuilder(shouldBuildActivity: false);

            //Local<ActivityLogNode> activityLocal = null;

            //decorate()
            //    .OnBefore(w => {
            //        activityLocal = w.Local<ActivityLogNode>();
            //    })
            //    .OnInputArgument((w, arg) => {
            //        inputLogNodeBuilder.AddNameValuePair(w.Const(arg.Name), arg.OperandType, arg, isDetail: true);
            //    })
            //    .OnInspectedInputArguments(w => {
            //        activityLocal.Assign(inputLogNodeBuilder.GetNewLogNodeOperand(w, _staticStrings.GetStaticStringOperand(messageId)).CastTo<ActivityLogNode>());
            //        _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal);
            //    })
            //    .OnException<Exception>((w, exception) => {
            //        activityLocal.CastTo<ILogActivity>().Void(x => x.Fail, exception);
            //        w.Throw();
            //    })
            //    .OnOutputArgument((w, arg) => {
            //        outputLogNodeBuilder.AddNameValuePair(w.Const(arg.Name), arg.OperandType, arg, isDetail: true);
            //    })
            //    .OnReturnValue((w, retVal) => {
            //        outputLogNodeBuilder.AddNameValuePair(
            //            Static.Prop(() => CallOutputReturnValueName), w.OwnerMethod.Signature.ReturnType, retVal, isDetail: true);
            //    })
            //    .OnSuccess(w => {
            //        if ( outputLogNodeBuilder.NameValuePairCount > 0 )
            //        {
            //            var logNodeOperand = outputLogNodeBuilder.GetNewLogNodeOperand(
            //                w, _staticStrings.GetStaticStringOperand(LoggingCallOutputsMessageId), w.Const(LogLevel.Debug), w.Const<Exception>(null));
            //            _threadLogAppenderField.Void(x => x.AppendLogNode, logNodeOperand);
            //        }
            //    })
            //    .OnAfter(w => {
            //        activityLocal.CastTo<ILogActivity>().Void(x => x.Dispose);
            //    });
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AspectProvider : IComponentAspectProvider
        {
            #region Implementation of IComponentAspectProvider

            public IObjectFactoryConvention GetAspectConvention(ComponentAspectFactory.ConventionContext context)
            {
                return new CallAuthorizationAspectConvention(context);
            }

            #endregion
        }
    }
}
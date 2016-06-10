using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly ComponentAspectFactory.ConventionContext _aspectContext;
        public static readonly string LoggingCallOutputsMessageId = "CallLoggingAspect.LoggingCallOutputs";
        public static readonly string CallOutputReturnValueName = "return";

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly StaticStringsDecorator _staticStrings;
        private readonly Dictionary<MethodInfo, MethodInfo> _targetInterfaceMap;
        private Field<Pipeline<IThreadLogAppender>> _threadLogAppenderPipelineField;
        private Field<IThreadLogAppender> _threadLogAppenderField;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public CallLoggingAspectConvention(ComponentAspectFactory.ConventionContext aspectContext)
            : base(Will.DecorateClass | Will.DecorateConstructors | Will.DecorateMethods)
        {
            _aspectContext = aspectContext;
            _staticStrings = aspectContext.StaticStrings;
            _targetInterfaceMap = new Dictionary<MethodInfo, MethodInfo>();

            BuildTargetInterfaceMap();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
            _threadLogAppenderPipelineField = _aspectContext.GetDependencyField<Pipeline<IThreadLogAppender>>(classWriter, "$appendersPipeline");
            _threadLogAppenderField = classWriter.Field<IThreadLogAppender>("$multicastAppender");
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnConstructor(MethodMember member, Func<ConstructorDecorationBuilder> decorate)
        {
            decorate().OnSuccess(w => 
                _threadLogAppenderField.Assign(_threadLogAppenderPipelineField.Func<IThreadLogAppender>(x => x.AsService))
            );
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
        {
            var messageId = LogNode.PreserveMessageIdPrefix + GetQualifiedMemberNameToLog(member);
            var inputLogNodeBuilder = new NameValuePairLogNodeBuilder(shouldBuildActivity: true);
            var outputLogNodeBuilder = new NameValuePairLogNodeBuilder(shouldBuildActivity: false);
                
            Local<ActivityLogNode> activityLocal = null;

            decorate()
                .OnBefore(w => {
                    activityLocal = w.Local<ActivityLogNode>();
                })
                .OnInputArgument((w, arg) => {
                    inputLogNodeBuilder.AddNameValuePair(_staticStrings.GetStaticStringOperand(arg.Name), arg.OperandType, arg, isDetail: true);
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
                    outputLogNodeBuilder.AddNameValuePair(_staticStrings.GetStaticStringOperand(arg.Name), arg.OperandType, arg, isDetail: true);
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

        private string GetQualifiedMemberNameToLog(MethodMember member)
        {
            string memberName = null;
            MethodInfo targetMethod;

            if (member.MethodDeclaration != null && _targetInterfaceMap.TryGetValue(member.MethodDeclaration, out targetMethod))
            {
                if (targetMethod.Name.IndexOf('.') > 0)
                {
                    var nameParts = targetMethod.Name.Split('.');
                    var isUniqueName = !_targetInterfaceMap.Values.Any(method => 
                        method != targetMethod &&
                        GetMemberSimpleName(method.Name) == nameParts[nameParts.Length - 1]);
                    var qualifierCount = (isUniqueName ? 0 : 1);
                    memberName = string.Join(".", nameParts.Skip(nameParts.Length - qualifierCount - 1));
                }
                else
                {
                    memberName = targetMethod.Name;
                }
            }

            return (
                _aspectContext.ComponentType.Name + 
                "." + 
                (memberName ?? member.Name));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetMemberSimpleName(string name)
        {
            if (name.IndexOf('.') > 0)
            {
                var nameParts = name.Split('.');
                return nameParts[nameParts.Length - 1];
            }

            return name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildTargetInterfaceMap()
        {
            var allInterfaceMaps = _aspectContext.ComponentType.GetInterfaces().Select(intf => _aspectContext.ComponentType.GetInterfaceMap(intf));

            foreach (var map in allInterfaceMaps)
            {
                for (int i = 0; i < map.InterfaceMethods.Length; i++)
                {
                    _targetInterfaceMap[map.InterfaceMethods[i]] = map.TargetMethods[i];
                }
            }
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
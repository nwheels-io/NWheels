using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Applied.Conventions;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Logging;

namespace NWheels.Core.Logging
{
    public class CallLoggingAspectFactory : ConventionObjectFactory
    {
        public CallLoggingAspectFactory(DynamicModule module)
            : base(module, context => new IObjectFactoryConvention[] { new CallTargetConvention(), new AspectConvention() })
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AspectConvention : DecorationConvention
        {
            private TypeKey _typeKey;
            private Field<IThreadLogAppender> _threadLogAppenderField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AspectConvention()
                : base(Will.DecorateClass | Will.DecorateMethods)
            {
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
                _threadLogAppenderField = classWriter.DependencyField<IThreadLogAppender>("_threadLogAppender");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
            {
                var messageId = _typeKey.PrimaryInterface.Name.TrimPrefix("I") + "::" + member.Name;
                Local<NameValuePairActivityLogNode> activity = null;

                decorate()
                    .OnBefore(w => {
                        activity = w.Local<NameValuePairActivityLogNode>();
                        activity.Assign(w.New<NameValuePairActivityLogNode>(w.Const(messageId)));
                        _threadLogAppenderField.Void(x => x.AppendActivityNode, activity);
                    })
                    .OnException<Exception>((w, exception) => {
                        activity.CastTo<ILogActivity>().Void(x => x.Fail, exception);
                        w.Throw();
                    })
                    .OnAfter(w => {
                        activity.CastTo<ILogActivity>().Void(x => x.Dispose);     
                    });
            }
        }
    }
}

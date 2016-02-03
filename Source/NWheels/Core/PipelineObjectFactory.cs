using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using TT = Hapil.TypeTemplate;

namespace NWheels.Core
{
    public class PipelineObjectFactory : ConventionObjectFactory
    {
        public PipelineObjectFactory(DynamicModule module)
            : base(module)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService GetPipelineAsServiceObject<TService>(Pipeline<TService> pipeline)
            where TService : class
        {
            if ( pipeline == null )
            {
                throw new ArgumentNullException("pipeline");
            }

            if ( !typeof(TService).IsInterface )
            {
                throw new ArgumentException("TService must be an interface", "TService");
            }

            var typeKey = new TypeKey(primaryInterface: typeof(TService));
            var typeEntry = GetOrBuildType(typeKey);
            var pipelineObject =  typeEntry.CreateInstance<TService, Pipeline<TService>>(0, pipeline);

            return pipelineObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new PipelineInvocationConvention()
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PipelineInvocationConvention : ImplementationConvention
        {
            public PipelineInvocationConvention()
                : base(Will.ImplementPrimaryInterface)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TT.TInterface> writer)
            {
                Field<IReadOnlyList<TT.TInterface>> sinksField;

                writer
                    .Field("_sinks", out sinksField)
                    .Constructor<IReadOnlyList<TT.TInterface>>((w, sinks) => {
                        sinksField.Assign(sinks);
                    })
                    .AllMethods(where: IsVoidMethodWithNoOutputParameters).Implement(w => {
                        w.For(from: w.Const(0), to: sinksField.Prop(x => x.Count)).Do((loop, index) => {
                            w.PropagateCall<object>(sinksField.Item<int, TT.TInterface>(index));
                        });
                    })
                    .AllMethods().Throw<NotSupportedException>("Non-void methods and methods with out parameters cannot be invoked on a pipeline.");
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static bool IsVoidMethodWithNoOutputParameters(MethodInfo method)
            {
                return (
                    method.IsVoid() &&
                    method.GetParameters().All(p => !p.ParameterType.IsByRef || !p.IsOut));
            }
        }
    }
}

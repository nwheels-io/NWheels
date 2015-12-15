using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class ContextImplTypeInjectionConvention : DecorationConvention
    {
        private readonly MongoEntityObjectFactory.ConventionContext _conventionContext;
        private Field<IComponentContext> _componentsField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContextImplTypeInjectionConvention(MongoEntityObjectFactory.ConventionContext conventionContext)
            : base(Will.DecorateClass | Will.DecorateMethods)
        {
            _conventionContext = conventionContext;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
            _conventionContext.ContextImplTypeField = classWriter.Field<Type>("$contextImplType");
            _componentsField = classWriter.DependencyField<IComponentContext>("$components");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
        {
            if ( member.MethodDeclaration == _s_injectDependenciesMethodInfo )
            {
                decorate().OnSuccess(w => {
                    var contextObjectLocal = w.Local<DataRepositoryBase>();
                    w.If(
                        Static.Func<bool>(_s_tryResolveDataRepositoryBaseMethodInfo, 
                            _componentsField, 
                            contextObjectLocal
                        )
                    ).Then(() => {
                        _conventionContext.ContextImplTypeField.Assign(contextObjectLocal.Func<Type>(x => x.GetType));
                    });
                });
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly MethodInfo _s_injectDependenciesMethodInfo =
            ExpressionUtility.GetMethodInfo<Expression<Action<IHaveDependencies, IComponentContext>>>((x, c) => x.InjectDependencies(c));

        private static readonly MethodInfo _s_tryResolveDataRepositoryBaseMethodInfo =
            ExpressionUtility.GetMethodInfo<Expression<Func<IComponentContext, DataRepositoryBase, bool>>>((c, v) => ResolutionExtensions.TryResolve(c, out v));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using System.Reflection;
using NWheels.Utilities;
using NWheels.Entities.Core;

namespace NWheels.Entities.Factories
{
    public class ActiveRecordConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _factoryContext;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ActiveRecordConvention(DomainObjectFactoryContext factoryContext)
            : base(Will.ImplementBaseClass)
        {
            _factoryContext = factoryContext;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            if ( DomainObjectImplementsActiveRecord() )
            { 
                writer.VoidMethods(where: IsActiveRecordSaveMethod).Implement(w => {
                    Static.Void(RuntimeEntityModelHelpers.SaveActiveRecordObject, w.This<IDomainObject>(), _factoryContext.FrameworkField);
                });
                writer.VoidMethods(where: IsActiveRecordDeleteMethod).Implement(w => {
                    Static.Void(RuntimeEntityModelHelpers.DeleteActiveRecordObject, w.This<IDomainObject>(), _factoryContext.FrameworkField);
                });
            }
            else
            { 
                writer.ImplementInterface<IActiveRecord>()
                    .Method(intf => intf.Save).Implement(w => {
                        Static.Void(RuntimeEntityModelHelpers.SaveActiveRecordObject, w.This<IDomainObject>(), _factoryContext.FrameworkField);
                    })
                    .Method(intf => intf.Delete).Implement(w => {
                        Static.Void(RuntimeEntityModelHelpers.DeleteActiveRecordObject, w.This<IDomainObject>(), _factoryContext.FrameworkField);
                    });
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool DomainObjectImplementsActiveRecord()
        {
            return (
                _factoryContext.MetaType.DomainObjectType != null && 
                typeof(IActiveRecord).IsAssignableFrom(_factoryContext.MetaType.DomainObjectType));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static MethodInfo _s_activeRecordSaveMethod = 
            ExpressionUtility.GetMethodInfo<Expression<Action<IActiveRecord>>>(x => x.Save());

        private static MethodInfo _s_activeRecordDeleteMethod = 
            ExpressionUtility.GetMethodInfo<Expression<Action<IActiveRecord>>>(x => x.Delete());

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static bool IsActiveRecordSaveMethod(MethodInfo method)
        {
            return (
                method.DeclaringType != null && 
                method.DeclaringType.IsClass && 
                method.IsAbstract && 
                !method.IsGenericMethod && 
                method.GetParameters().Length == 0 && 
                method.Name == _s_activeRecordSaveMethod.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsActiveRecordDeleteMethod(MethodInfo method)
        {
            return (
                method.DeclaringType != null &&
                method.DeclaringType.IsClass &&
                method.IsAbstract &&
                !method.IsGenericMethod &&
                method.GetParameters().Length == 0 &&
                method.Name == _s_activeRecordDeleteMethod.Name);
        }
    }
}

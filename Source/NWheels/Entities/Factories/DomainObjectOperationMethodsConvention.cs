using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects;

namespace NWheels.Entities.Factories
{
    public class DomainObjectOperationMethodsConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectOperationMethodsConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementPrimaryInterface)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_context.MetaType.DomainObjectType == null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.AllMethods().ForEach(method => {
                if ( !IsImplementedByDomainObject(method) )
                {
                    writer.Method(method).Implement(w => w.Throw<NotSupportedException>("Entity methods are not supported by automatic domain objects."));
                }
            });
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsImplementedByDomainObject(MethodInfo method)
        {
            for ( var metaType = _context.MetaType ; metaType != null ; metaType = metaType.BaseType )
            {
                if ( metaType.DomainObjectType != null &&
                    TypeMemberCache.Of(metaType.DomainObjectType)
                        .Methods.Where(m => m.Name == method.Name)
                        .OfSignature(method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray())
                        .Any() )
                {
                    return true;
                }
            }

            return false;
        }
    }
}


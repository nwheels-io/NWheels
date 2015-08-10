using Hapil;
using Hapil.Writers;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;

namespace NWheels.Entities.Factories
{
    public class ContainedPersistableObjectConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ContainedPersistableObjectConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementBaseClass)
        {
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            writer.ImplementInterfaceExplicitly<IDomainObject>();
            writer.ImplementInterfaceExplicitly<IContain<IPersistableObject>>()
                .Method<IPersistableObject>(intf => intf.GetContainedObject).Implement(w =>
                    w.Return(_context.PersistableObjectField.CastTo<IPersistableObject>())
                );
        }

        #endregion
    }
}
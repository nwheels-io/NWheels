using Hapil;

namespace NWheels.Entities.Factories
{
    public class DomainObjectBaseTypeConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _conventionState;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectBaseTypeConvention(DomainObjectFactoryContext conventionState)
            : base(Will.InspectDeclaration)
        {
            _conventionState = conventionState;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            context.BaseType = _conventionState.MetaType.DomainObjectType;
        }

        #endregion
    }
}
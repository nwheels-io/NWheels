using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIPresentationObjectConvention : ImplementationConvention
    {
        private readonly PresentationObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIPresentationObjectConvention(PresentationObjectFactoryContext context)
            : base(Will.ImplementBaseClass)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
        {
            writer.ImplementInterface<IPresentationObject>()
                .Method<IDomainObject>(intf => intf.GetDomainObject).Implement(w => 
                    w.Return(_context.DomainObjectField.CastTo<IDomainObject>())
                );

            writer.ImplementInterfaceExplicitly<IObject>()
                .Property(intf => intf.IsModified).ImplementPropagate(_context.DomainObjectField.CastTo<IObject>());

            ImplementToString(writer);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementToString(ImplementationClassWriter<TT.TBase> writer)
        {
            if ( _context.MetaType.BaseType == null )
            {
                writer.Method<string>(x => x.ToString).Implement(w => {
                    w.Return(_context.DomainObjectField.FuncToString());
                });
            }
        }
    }
}
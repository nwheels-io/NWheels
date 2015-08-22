using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using NWheels.DataObjects;

namespace NWheels.Entities.Factories
{
    public class DomainObjectModifiedVectorConvention : DecorationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectModifiedVectorConvention(DomainObjectFactoryContext context)
            : base(Will.DecorateProperties)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
        {
            IPropertyMetadata metaProperty;

            if ( _context.MetaType.TryGetPropertyByDeclaration(member.PropertyDeclaration, out metaProperty) )
            {
                if ( metaProperty.Kind == PropertyKind.Scalar && metaProperty.ContractPropertyInfo.CanWrite )
                {
                    decorate().Setter().OnBefore(w => _context.ModifiedVector.WriteSetBit(w, metaProperty.PropertyIndex));
                }
            }
        }

        #endregion
    }
}

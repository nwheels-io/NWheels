using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class EfNotMappedConvention : DecorationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfNotMappedConvention(ITypeMetadata metaType)
            : base(Will.DecorateProperties)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return _metaType.ContractType.IsEntityPartContract();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
        {
            Type collectionItemType;
            bool shouldNotMap;

            if ( member.PropertyType.IsCollectionType(out collectionItemType) )
            {
                shouldNotMap = collectionItemType.IsClass && collectionItemType.GetInterfaces().Any(t => t.IsEntityContract());
            }
            else
            {
                shouldNotMap = member.PropertyType.IsClass && member.PropertyType.GetInterfaces().Any(t => t.IsEntityContract());
            }

            if ( shouldNotMap )
            {
                decorate().Attribute<NotMappedAttribute>();
            }
        }
    }
}

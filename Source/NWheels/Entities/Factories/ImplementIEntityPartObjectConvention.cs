using System;
using System.Linq;
using Autofac;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIEntityPartObjectConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIEntityPartObjectConvention(ITypeMetadata metaType)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return context.TypeKey.PrimaryInterface.IsEntityPartContract();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var domainObjectField = writer.Field<IDomainObject>("$domainObject");

            writer.ImplementInterfaceExplicitly<IEntityPartObject>()
                .Method<object[]>(intf => intf.ExportValues).Throw<NotImplementedException>()
                .Method<object[]>(intf => intf.ImportValues).Throw<NotImplementedException>();

                //.Method<IDomainObject>(x => x.SetContainerObject).Implement((m, domainObject) => {
                //    domainObjectField.Assign(domainObject);
                //})
                //.Method<IDomainObject>(x => x.GetContainerObject).Implement(m => {
                //    m.Return(domainObjectField);
                //});

            ImplementToString(writer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementToString(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var displayProperty = _metaType.DefaultDisplayProperties.FirstOrDefault();

            writer.Method<string>(x => x.ToString).Implement(w => {
                w.Return(
                    w.Const(_metaType.Name + (displayProperty != null ? "[" : "")) +
                    TypeFactoryUtility.GetPropertyStringValueOperand(w, displayProperty) +
                    (displayProperty != null ? "]" : ""));
            });
        }

        #endregion
    }
}

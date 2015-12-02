using NWheels.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Toolbox;

namespace NWheels.UI.Uidl
{
    public static class UidlUtility
    {
        public static WidgetUidlNode CreateFormOrTypeSelector(ITypeMetadata metaType, string idName, ControlledUidlNode parent, bool isInline)
        {
            var availableConcreteTypes = GetAvailableConcreteTypes(metaType);

            if ( ShouldCreateFormTypeSelector(metaType, availableConcreteTypes) )
            {
                return TypeSelector.Create(
                    idName + "Type",
                    parent,
                    metaType,
                    availableConcreteTypes,
                    concreteType => CreateForm(concreteType, idName, parent, isInline));
            }
            else
            {
                return CreateForm(metaType, idName, parent, isInline);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldCreateFormTypeSelector(ITypeMetadata metaType)
        {
            var availableConcreteTypes = GetAvailableConcreteTypes(metaType);
            return ShouldCreateFormTypeSelector(metaType, availableConcreteTypes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static WidgetUidlNode CreateForm(ITypeMetadata metaType, string idName, ControlledUidlNode parent, bool isInline)
        {
            var closedType = typeof(Form<>).MakeGenericType(metaType.ContractType);
            return (WidgetUidlNode)Activator.CreateInstance(closedType, idName + "<" + metaType.Name + ">", parent, isInline);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ITypeMetadata[] GetAvailableConcreteTypes(ITypeMetadata metaType)
        {
            return new[] { metaType }.Concat(metaType.DerivedTypes).Where(t => !t.IsAbstract).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool ShouldCreateFormTypeSelector(ITypeMetadata metaType, ITypeMetadata[] availableConcreteTypes)
        {
            return (availableConcreteTypes.Length != 1 || availableConcreteTypes[0] != metaType);
        }
    }
}

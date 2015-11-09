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
            if ( metaType.DerivedTypes.Count == 0 )
            {
                return CreateCrudForm(metaType, idName, parent, isInline);
            }
            else
            {
                return TypeSelector.Create(
                    idName + "Type",
                    parent,
                    metaType,
                    concreteType => CreateCrudForm(concreteType, idName, parent, isInline));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static WidgetUidlNode CreateCrudForm(ITypeMetadata metaType, string idName, ControlledUidlNode parent, bool isInline)
        {
            var closedType = typeof(Form<>).MakeGenericType(metaType.ContractType);
            return (WidgetUidlNode)Activator.CreateInstance(closedType, idName, parent, isInline);
        }
    }
}

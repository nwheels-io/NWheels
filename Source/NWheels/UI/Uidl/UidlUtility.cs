﻿using NWheels.DataObjects;
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
        public static WidgetUidlNode CreateFormOrTypeSelector(ITypeMetadata metaType, string idName, ControlledUidlNode parent, bool isNested)
        {
            if ( metaType.DerivedTypes.Count == 0 )
            {
                return CreateCrudForm(metaType, idName, parent, isNested);
            }
            else
            {
                return TypeSelector.Create(
                    idName + "Type",
                    parent,
                    metaType,
                    concreteType => CreateCrudForm(concreteType, idName, parent, isNested));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static WidgetUidlNode CreateCrudForm(ITypeMetadata metaType, string idName, ControlledUidlNode parent, bool isNested)
        {
            var closedType = typeof(CrudForm<,,>).MakeGenericType(
                metaType.ContractType,
                typeof(Empty.Data),
                typeof(ICrudFormState<>).MakeGenericType(metaType.ContractType));
            return (WidgetUidlNode)Activator.CreateInstance(closedType, idName, parent, isNested);
        }
    }
}
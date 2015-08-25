using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using TT = Hapil.TypeTemplate;

namespace NWheels.Utilities
{
    public static class TypeFactoryUtility
    {
        public static Operand<string> GetPropertyStringValueOperand(MethodWriterBase writer, IPropertyMetadata property)
        {
            return GetPropertyStringValueOperand(writer, property != null ? property.ContractPropertyInfo : null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Operand<string> GetPropertyStringValueOperand(MethodWriterBase writer, PropertyInfo property)
        {
            var w = writer;

            if ( property == null )
            {
                return w.Const("");
            }

            using ( TT.CreateScope<TT.TProperty>(property.PropertyType) )
            {
                if ( property.PropertyType.IsValueType )
                {
                    return w.This<TT.TBase>().Prop<TT.TProperty>(property).FuncToString();
                }
                else
                {
                    return w.Iif(
                        w.This<TT.TBase>().Prop<TT.TProperty>(property).IsNotNull(),
                        w.This<TT.TBase>().Prop<TT.TProperty>(property).FuncToString(),
                        w.Const(""));
                }
            }
        }
    }
}

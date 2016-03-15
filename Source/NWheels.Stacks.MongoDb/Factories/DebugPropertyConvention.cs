using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class DebugPropertyConvention : DecorationConvention
    {
        public DebugPropertyConvention()
            : base(Will.DecorateProperties)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
        {
            if (ShouldDebug(member.PropertyBuilder))
            {
                decorate().Getter()
                    .OnBefore(d => {
                        using (TT.CreateScope<TT.TProperty>(member.PropertyType))
                        {
                            Static.GenericVoid(s => BeforeGetValue<TT.TProperty>(s), d.Const(member.Name));
                        }
                    })
                    .OnReturnValue((d, retVal) => {
                        using (TT.CreateScope<TT.TProperty>(member.PropertyType))
                        {
                            Static.GenericVoid((s, v) => AfterGetValue(s, v), d.Const(member.Name), retVal.CastTo<TT.TProperty>());
                        }
                    });

                decorate().Setter()
                    .OnBefore(d => {
                        using (TT.CreateScope<TT.TProperty>(member.PropertyType))
                        {
                            Static.GenericVoid((s, v) => BeforeSetValue(s, v), d.Const(member.Name), d.Arg1<TT.TProperty>());
                        }
                    })
                    .OnReturnVoid(d => {
                        using (TT.CreateScope<TT.TProperty>(member.PropertyType))
                        {
                            Static.GenericVoid((s, v) => AfterSetValue(s, v), d.Const(member.Name), d.Arg1<TT.TProperty>());
                        }
                    });
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldDebug(PropertyInfo p)
        {
            return (p.Name.EndsWith("AssociatedRoles"));
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void BeforeGetValue<T>(string propertyName)
        {
        }
        public static void AfterGetValue<T>(string propertyName, T value)
        {
        }
        public static void BeforeSetValue<T>(string propertyName, T value)
        {
        }
        public static void AfterSetValue<T>(string propertyName, T value)
        {
        }
    }
}

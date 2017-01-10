using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Policy.Relaxed;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Policy.Common
{
    public class EqualityComparerConvention : ConventionBase<object, object>
    {
        protected override void Implement(ITypeFactoryContext<object, object> context, TypeWriter writer)
        {
            //using (writer.DefineTypePlaceholder<TT>(context.Key.PrimaryContract))
            //{
            //    IComparable<string> c;

            //    var implement = writer.ImplementInterface<IEqualityComparer<TT>>();

            //    implement.NonVoidMethod<TT, TT, bool>(T => T.Equals)
            //        .Body((Generator G, Arg x, Arg y) => {



            //            G.IF(x == y).THEN(() => {
            //                G.RETURN(true);
            //            }).ELSE(() => {
            //                G.RETURN(false);
            //            });
            //        });

            //    implement.NonVoidMethod<int>(T => T.GetHashCode)
            //        .Body(Generator G => {
            //            G.RETURN(0);
            //        });
            //}

            var z = 128;

            while (true)
            {
                var n = 123;
                var x = GetComparisonExpression(null, null);

                AbstractExpression GetComparisonExpression(AbstractMember a, AbstractMember b)
                {
                    var s = n.ToString() + z.ToString();
                    return null;
                }

            }

        }


        //[TypePlaceholder]
        private class TT
        {
        }
    }
}


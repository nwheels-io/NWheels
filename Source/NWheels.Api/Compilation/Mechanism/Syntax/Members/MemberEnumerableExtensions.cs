using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public static class MemberEnumerableExtensions
    {
        public static IEnumerable<TMember> Select<TMember>(this TypeMember type, Func<TMember, bool> where = null)
            where TMember : AbstractMember
        {
            var selector = type.Members.OfType<TMember>();

            if (where != null)
            {
                selector = selector.Where(where);
            }

            return selector;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<TMember> SelectPublicInstance<TMember>(this TypeMember type, Func<TMember, bool> where = null)
            where TMember : AbstractMember
        {
            var selector = 
                Select<TMember>(type, where)
                .Where(m => 
                    m.Visibility == MemberVisibility.Public && 
                    (m.Modifier & MemberModifier.Static) == 0
                );

            return selector;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static HavingSelector<TMember> Having<TMember>(this IEnumerable<TMember> source)
            where TMember : AbstractMember
        {
            return new HavingSelector<TMember>(source);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HavingSelector<TMember> 
            where TMember : AbstractMember
        {
            private readonly IEnumerable<TMember> _source;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HavingSelector(IEnumerable<TMember> source)
            {
                _source = source;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<TMember> Attribute<TAttribute>()
                where TAttribute : Attribute
            {
                return _source.Where(t => t.Attributes.Any(a => a.AttributeType == typeof(TAttribute)));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;

namespace NWheels.Conventions.Core
{
    public class StaticStringsDecorator : DecorationConvention
    {
        private readonly Dictionary<string, Field<string>> _staticStringFields;
        private DecoratingClassWriter _classWriter;
        private bool _staticConstructorImplemented;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public StaticStringsDecorator()
            : base(Will.DecorateClass | Will.DecorateConstructors)
        {
            _staticStringFields = new Dictionary<string, Field<string>>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IOperand<string> GetStaticStringOperand(string s)
        {
            Field<string> field;

            if ( !_staticStringFields.TryGetValue(s, out field) )
            {
                var fieldNameSuffix = new string(s.Where(IsValidFieldNameChar).ToArray());
                field = _classWriter.StaticField<string>("sstr$" + fieldNameSuffix);
                _staticStringFields.Add(s, field);
            }

            return field;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
            _classWriter = classWriter;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnStaticConstructor(MethodMember member, Func<ConstructorDecorationBuilder> decorate)
        {
            if ( !_staticConstructorImplemented )
            {
                AssignStaticStringFields();
                _staticConstructorImplemented = true;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnFinalizeDecoration(ClassType classType, DecoratingClassWriter classWriter)
        {
            if ( !_staticConstructorImplemented )
            {
                classWriter.ImplementBase<object>().StaticConstructor(cw => {
                    AssignStaticStringFields();
                });

                _staticConstructorImplemented = true;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssignStaticStringFields()
        {
            foreach ( var staticStringField in _staticStringFields )
            {
                staticStringField.Value.Assign(staticStringField.Key);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsValidFieldNameChar(char c)
        {
            return (
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_');
        }
    }
}

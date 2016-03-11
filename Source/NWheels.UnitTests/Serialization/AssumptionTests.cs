using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NUnit.Framework;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Serialization
{
    [TestFixture]
    public class AssumptionTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanAccessPrivateFieldsOfStructsWithDynamicMethod()
        {

            var compiler = new DynamicMethodCompiler(base.DyamicModule);
            var dynamicMethod = compiler.CompileStaticFunction<KeyValuePair<int, string>, int, string, KeyValuePair<int, string>>(
                "Test_AccessPrivateFieldsOfStructsWithDynamicMethod",
                (w, kvp, newKey, newValue) => {
                    Static.Prop(() => _s_oldKey).Assign(kvp.Field<int>(kvpKeyField));
                    Static.Prop(() => _s_oldValue).Assign(kvp.Field<string>(kvpValueField));
                    kvp.Field<int>(kvpKeyField).Assign(newKey);
                    kvp.Field<string>(kvpValueField).Assign(newValue);
                    w.Return(kvp);
                });

            _s_oldKey = 0;
            _s_oldValue = null;

            var newKvp = dynamicMethod(new KeyValuePair<int, string>(123, "ABC"), 456, "DEF");

            _s_oldKey.ShouldBe(123);
            _s_oldValue.ShouldBe("ABC");
            newKvp.Key.ShouldBe(456);
            newKvp.Value.ShouldBe("DEF");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateInstanceOfStructWithoutConstructor()
        {
            var compiler = new DynamicMethodCompiler(base.DyamicModule);
            var dynamicMethod = compiler.CompileStaticFunction<KeyValuePair<int, string>>(
                "Test_CreateInstanceOfStructWithoutConstructor",
                w => {
                    var kvp = w.Local(initialValue: Static.Func(
                        FormatterServices.GetSafeUninitializedObject, 
                        w.Const(typeof(KeyValuePair<int, string>))
                    ).CastTo<KeyValuePair<int, string>>());

                    kvp.Field<int>(kvpKeyField).Assign(123);
                    kvp.Field<string>(kvpValueField).Assign("ABC");
                    w.Return(kvp);
                });

            var newKvp = dynamicMethod();

            newKvp.Key.ShouldBe(123);
            newKvp.Value.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int _s_oldKey;
        private static string _s_oldValue;
        private static readonly FieldInfo kvpKeyField = typeof(KeyValuePair<int, string>).GetField("key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo kvpValueField = typeof(KeyValuePair<int, string>).GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}

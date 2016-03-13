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
            var dynamicMethod = (Func<KeyValuePair<int, string>, int, string, KeyValuePair<int, string>>)
                compiler.ForTemplatedDelegate<Func<KeyValuePair<int, string>, int, string, KeyValuePair<int, string>>>()
                .CompileStaticFunction<KeyValuePair<int, string>, int, string, KeyValuePair<int, string>>(
                    "Test_AccessPrivateFieldsOfStructsWithDynamicMethod",
                    (w, kvp, newKey, newValue) => {
                        Static.Prop(() => _s_oldKey).Assign(kvp.Field<int>(_s_kvpKeyField));
                        Static.Prop(() => _s_oldValue).Assign(kvp.Field<string>(_s_kvpValueField));
                        kvp.Field<int>(_s_kvpKeyField).Assign(newKey);
                        kvp.Field<string>(_s_kvpValueField).Assign(newValue);
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
            var dynamicMethod = (Func<KeyValuePair<int, string>>)compiler.ForTemplatedDelegate<Func<KeyValuePair<int, string>>>()
                .CompileStaticFunction<KeyValuePair<int, string>>(
                    "Test_CreateInstanceOfStructWithoutConstructor",
                    w => {
                        var kvp = w.Local(initialValue:
                            Static.Func(FormatterServices.GetSafeUninitializedObject, w.Const(typeof(KeyValuePair<int, string>)))
                            .CastTo<KeyValuePair<int, string>>());

                        kvp.Field<int>(_s_kvpKeyField).Assign(123);
                        kvp.Field<string>(_s_kvpValueField).Assign("ABC");
                        w.Return(kvp);
                    });

            var newKvp = dynamicMethod();

            newKvp.Key.ShouldBe(123);
            newKvp.Value.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanMutateFieldsOfStructPassedByRef()
        {
            var compiler = new DynamicMethodCompiler(base.DyamicModule);
            var dynamicMethod = (TestStructMutator)compiler.ForTemplatedDelegate<TestStructMutator>().CompileStaticVoidMethod<TestStruct>(
                "Test_MutateFieldsOfStructPassedByRef",
                (w, data) => {
                    data.Field(x => x.Str).Assign("ZZZ");
                    data.Field(x => x.Num).Assign(999);
                });

            var testData = new TestStruct() {
                Str = "ABC",
                Num = 123
            };

            dynamicMethod(ref testData);

            testData.Str.ShouldBe("ZZZ");
            testData.Num.ShouldBe(999);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int _s_oldKey;
        private static string _s_oldValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly FieldInfo _s_kvpKeyField = typeof(KeyValuePair<int, string>).GetField(
            "key",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo _s_kvpValueField = typeof(KeyValuePair<int, string>).GetField(
            "value",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void TestStructMutator(ref TestStruct data);

        public struct TestStruct
        {
            public string Str;
            public int Num;
        }
    }
}

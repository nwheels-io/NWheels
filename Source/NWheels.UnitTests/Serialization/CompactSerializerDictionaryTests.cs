using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Authorization;
using NWheels.Serialization;
using NWheels.Testing;
using NWheels.Utilities;
using Shouldly;
using Repo = NWheels.UnitTests.Serialization.TestObjectRepository;

namespace NWheels.UnitTests.Serialization
{
    [TestFixture]
    public class CompactSerializerDictionaryTests : UnitTestBase
    {
        [Test]
        public void CanRegisterTypeAndLookupByKey()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            //-- act

            int key1;
            int key2;

            dictionary.RegisterType(typeof(Repo.DerivedClassOne), out key1);
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo), out key2);

            var type1 = dictionary.LookupTypeOrThrow(key1);
            var type2 = dictionary.LookupTypeOrThrow(key2);

            //-- assert

            type1.ShouldBe(typeof(Repo.DerivedClassOne));
            type2.ShouldBe(typeof(Repo.DerivedClassTwo));

            key1.ShouldBeGreaterThan(0);
            key2.ShouldBeGreaterThan(0);
            key1.ShouldNotBe(key2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanRegisterMemberAndLookupByKey()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();
            var derivedOneIntValue = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.DerivedClassOne, object>>>(x => x.IntValue);
            var derivedTwoDateTimeValue = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.DerivedClassTwo, object>>>(x => x.DateTimeValue);

            //-- act

            int key1;
            int key2;

            dictionary.RegisterMember(derivedOneIntValue, out key1);
            dictionary.RegisterMember(derivedTwoDateTimeValue, out key2);

            var member1 = dictionary.LookupMemberOrThrow(key1);
            var member2 = dictionary.LookupMemberOrThrow(key2);

            //-- assert

            member1.ShouldBe(derivedOneIntValue);
            member2.ShouldBe(derivedTwoDateTimeValue);

            key1.ShouldBeGreaterThan(0);
            key2.ShouldBeGreaterThan(0);
            key1.ShouldNotBe(key2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanRegisterMultipleMembersOfType()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();
            var intValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.IntValue);
            var dateTimeValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.DateTimeValue);

            //-- act

            int key1;
            int key2;

            dictionary.RegisterMember(intValueProperty, out key1);
            dictionary.RegisterMember(dateTimeValueProperty, out key2);

            var member1 = dictionary.LookupMemberOrThrow(key1);
            var member2 = dictionary.LookupMemberOrThrow(key2);

            //-- assert

            member1.ShouldBe(intValueProperty);
            member2.ShouldBe(dateTimeValueProperty);

            key1.ShouldBeGreaterThan(0);
            key2.ShouldBeGreaterThan(0);
            key1.ShouldNotBe(key2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MultipleAttemptsToRegisterTypeReturnSameKey()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            //-- act

            int key1;
            int key2;
            int key2B;

            dictionary.RegisterType(typeof(Repo.DerivedClassOne), out key1);
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo), out key2);
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo), out key2B);

            var type1 = dictionary.LookupTypeOrThrow(key1);
            var type2 = dictionary.LookupTypeOrThrow(key2);
            var type2B = dictionary.LookupTypeOrThrow(key2B);

            //-- assert

            key2B.ShouldBe(key2);

            type1.ShouldBe(typeof(Repo.DerivedClassOne));
            type2.ShouldBe(typeof(Repo.DerivedClassTwo));
            type2B.ShouldBe(typeof(Repo.DerivedClassTwo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MultipleAttemptsToRegisterMemberReturnSameKey()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();
            var intValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.IntValue);
            var dateTimeValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.DateTimeValue);

            //-- act

            int key1;
            int key2;
            int key2B;

            dictionary.RegisterMember(intValueProperty, out key1);
            dictionary.RegisterMember(dateTimeValueProperty, out key2);
            dictionary.RegisterMember(dateTimeValueProperty, out key2B);

            var member1 = dictionary.LookupMemberOrThrow(key1);
            var member2 = dictionary.LookupMemberOrThrow(key2);
            var member2B = dictionary.LookupMemberOrThrow(key2B);

            //-- assert

            key2B.ShouldBe(key2);

            member1.ShouldBeSameAs(intValueProperty);
            member2.ShouldBe(dateTimeValueProperty);
            member2B.ShouldBe(dateTimeValueProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThrowOnLookupTypeByNonExistentKey()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            int key1;
            int key2;
            dictionary.RegisterType(typeof(Repo.DerivedClassOne), out key1);
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo), out key2);

            //-- act & assert

            Should.Throw<CompactSerializerException>(
                () => {
                    dictionary.LookupTypeOrThrow(12345);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThrowOnLookupKeyByNonExistentType()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            int key1;
            int key2;
            dictionary.RegisterType(typeof(Repo.DerivedClassOne), out key1);
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo), out key2);

            //-- act & assert

            Should.Throw<CompactSerializerException>(
                () => {
                    dictionary.LookupTypeKeyOrThrow(typeof(InstanceContext));
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThrowOnLookupMemberByNonExistentKey()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            var intValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.IntValue);
            var dateTimeValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.DateTimeValue);

            int key1;
            int key2;
            dictionary.RegisterMember(intValueProperty, out key1);
            dictionary.RegisterMember(dateTimeValueProperty, out key2);

            //-- act & assert

            Should.Throw<CompactSerializerException>(
                () => {
                    dictionary.LookupMemberOrThrow(12345);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThrowOnLookupKeyByNonExistentMember()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            var intValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.IntValue);
            var dateTimeValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.DateTimeValue);
            var guidValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.GuidValue);

            int key1;
            int key2;
            dictionary.RegisterMember(intValueProperty, out key1);
            dictionary.RegisterMember(dateTimeValueProperty, out key2);

            //-- act & assert

            Should.Throw<CompactSerializerException>(
                () => {
                    dictionary.LookupMemberKeyOrThrow(guidValueProperty);
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLookupKeyByType()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();

            int registeredKey1;
            int registeredKey2;
            dictionary.RegisterType(typeof(Repo.DerivedClassOne), out registeredKey1);
            dictionary.RegisterType(typeof(Repo.DerivedClassTwo), out registeredKey2);

            //-- act

            var resultKey1 = dictionary.LookupTypeKeyOrThrow(typeof(Repo.DerivedClassOne));
            var resultKey2 = dictionary.LookupTypeKeyOrThrow(typeof(Repo.DerivedClassTwo));

            //-- assert

            resultKey1.ShouldBe(registeredKey1);
            resultKey2.ShouldBe(registeredKey2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLookupKeyByMember()
        {
            //-- arrange

            var dictionary = CreateDictionaryUnderTest();
            var derivedOneIntValue = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.DerivedClassOne, object>>>(x => x.IntValue);
            var derivedTwoDateTimeValue = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.DerivedClassTwo, object>>>(x => x.DateTimeValue);

            int registeredKey1;
            int registeredKey2;
            dictionary.RegisterMember(derivedOneIntValue, out registeredKey1);
            dictionary.RegisterMember(derivedTwoDateTimeValue, out registeredKey2);

            //-- act

            var resultKey1 = dictionary.LookupMemberKeyOrThrow(derivedOneIntValue);
            var resultKey2 = dictionary.LookupMemberKeyOrThrow(derivedTwoDateTimeValue);

            //-- assert

            resultKey1.ShouldBe(registeredKey1);
            resultKey2.ShouldBe(registeredKey2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeAndDeserializeDictionary()
        {
            //-- arrange

            var originalDictionary = CreateDictionaryUnderTest();
            var intValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.IntValue);
            var dateTimeValueProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<Repo.Primitive, object>>>(x => x.DateTimeValue);

            int derivedOneTypeKey;
            int derivedTwoTypeKey;
            int intValuePropertyKey;
            int dateTimeValuePropertyKey;

            originalDictionary.RegisterType(typeof(Repo.DerivedClassOne), out derivedOneTypeKey);
            originalDictionary.RegisterType(typeof(Repo.DerivedClassTwo), out derivedTwoTypeKey);
            originalDictionary.RegisterMember(intValueProperty, out intValuePropertyKey);
            originalDictionary.RegisterMember(dateTimeValueProperty, out dateTimeValuePropertyKey);

            //-- act

            var serializedStream = new MemoryStream();
            
            using (var writer = new CompactBinaryWriter(serializedStream))
            {
                originalDictionary.WriteTo(writer);
            }

            var serializedBytes = serializedStream.ToArray();
            var deserializedDictionary = CreateDictionaryUnderTest();

            using (var reader = new CompactBinaryReader(new MemoryStream(serializedBytes)))
            {
                deserializedDictionary.ReadFrom(reader);
            }

            //-- assert

            deserializedDictionary.LookupTypeOrThrow(derivedOneTypeKey).ShouldBe(typeof(Repo.DerivedClassOne));
            deserializedDictionary.LookupTypeOrThrow(derivedTwoTypeKey).ShouldBe(typeof(Repo.DerivedClassTwo));
            deserializedDictionary.LookupMemberOrThrow(intValuePropertyKey).ShouldBeSameAs(intValueProperty);
            deserializedDictionary.LookupMemberOrThrow(dateTimeValuePropertyKey).ShouldBeSameAs(dateTimeValueProperty);

            deserializedDictionary.LookupTypeKeyOrThrow(typeof(Repo.DerivedClassOne)).ShouldBe(derivedOneTypeKey);
            deserializedDictionary.LookupTypeKeyOrThrow(typeof(Repo.DerivedClassTwo)).ShouldBe(derivedTwoTypeKey);
            deserializedDictionary.LookupMemberKeyOrThrow(intValueProperty).ShouldBe(intValuePropertyKey);
            deserializedDictionary.LookupMemberKeyOrThrow(dateTimeValueProperty).ShouldBe(dateTimeValuePropertyKey);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanDeserializeDictionaryWithTypesFromDifferentAssemblies()
        {
            //-- arrange

            var originalDictionary = CreateDictionaryUnderTest();
            var sessionProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<IAccessControlContext, object>>>(x => x.Session);
            var userStoryProperty = ExpressionUtility.GetPropertyInfo<Expression<Func<IAccessControlContext, object>>>(x => x.UserStory);

            int operationContextTypeKey;
            int instanceContextTypeKey;
            int sessionPropertyKey;
            int userStoryPropertyKey;

            originalDictionary.RegisterType(typeof(OperationContext), out operationContextTypeKey);
            originalDictionary.RegisterType(typeof(InstanceContext), out instanceContextTypeKey);
            originalDictionary.RegisterMember(sessionProperty, out sessionPropertyKey);
            originalDictionary.RegisterMember(userStoryProperty, out userStoryPropertyKey);

            //-- act

            var serializedStream = new MemoryStream();

            using (var writer = new CompactBinaryWriter(serializedStream))
            {
                originalDictionary.WriteTo(writer);
            }

            var serializedBytes = serializedStream.ToArray();
            var deserializedDictionary = CreateDictionaryUnderTest();

            using (var reader = new CompactBinaryReader(new MemoryStream(serializedBytes)))
            {
                deserializedDictionary.ReadFrom(reader);
            }

            //-- assert

            deserializedDictionary.LookupTypeOrThrow(operationContextTypeKey).ShouldBe(typeof(OperationContext));
            deserializedDictionary.LookupTypeOrThrow(instanceContextTypeKey).ShouldBe(typeof(InstanceContext));
            deserializedDictionary.LookupMemberOrThrow(sessionPropertyKey).ShouldBeSameAs(sessionProperty);
            deserializedDictionary.LookupMemberOrThrow(userStoryPropertyKey).ShouldBeSameAs(userStoryProperty);

            deserializedDictionary.LookupTypeKeyOrThrow(typeof(OperationContext)).ShouldBe(operationContextTypeKey);
            deserializedDictionary.LookupTypeKeyOrThrow(typeof(InstanceContext)).ShouldBe(instanceContextTypeKey);
            deserializedDictionary.LookupMemberKeyOrThrow(sessionProperty).ShouldBe(sessionPropertyKey);
            deserializedDictionary.LookupMemberKeyOrThrow(userStoryProperty).ShouldBe(userStoryPropertyKey);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanRegisterApiContract()
        {
            //-- arrange

            var contractMembers = typeof(Repo.IApiContract).GetMembers().Where(IsDeclaredMember).ToArray();
            var dictionary = CreateDictionaryUnderTest();

            //-- act

            dictionary.RegisterApiContract(typeof(Repo.IApiContract));

            //-- assert

            contractMembers.Length.ShouldBe(10);
            var distinctKeys = new HashSet<int>();

            foreach (var member in contractMembers)
            {
                int key;
                dictionary.TryLookupMemberKey(member, out key).ShouldBeTrue("Member [" + member.Name + "] was expected to be registered, but it was not.");
                distinctKeys.Add(key).ShouldBeTrue();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual CompactSerializerDictionary CreateDictionaryUnderTest()
        {
            return new StaticCompactSerializerDictionary();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsDeclaredMember(MemberInfo member)
        {
            var methodInfo = member as MethodInfo;

            if (methodInfo != null)
            {
                return ((methodInfo.Attributes & MethodAttributes.SpecialName) == 0);
            }

            return true;
        }
    }
}

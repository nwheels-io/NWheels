using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Endpoints.Core;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Serialization;
using NWheels.Serialization.Factories;
using NWheels.Testing;
using Shouldly;
using Repo = NWheels.UnitTests.Serialization.TestObjectRepository;

namespace NWheels.UnitTests.Endpoints.Core
{
    [TestFixture]
    public class CompactRpcProtocolTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void Roundtrip_WriteAndReadCall()
        {
            //-- arrange

            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var dictionary = new StaticCompactSerializerDictionary();
            dictionary.RegisterApiContract(typeof(Repo.IApiContract));
            dictionary.MakeImmutable();

            var networkStream = new MemoryStream();
            var writerContext = new CompactSerializationContext(
                serializer,
                dictionary,
                new CompactBinaryWriter(networkStream));
            var readerContext = new CompactDeserializationContext(
                serializer,
                dictionary,
                new CompactBinaryReader(networkStream),
                Framework.Components);

            var callMethod = typeof(Repo.IApiContract).GetMethod("VoidOperationWithParameters");

            //-- act

            var originalCall = Resolve<IMethodCallObjectFactory>().NewMessageCallObject(callMethod);
            originalCall.SetParameterValue(0, 123);
            originalCall.SetParameterValue(1, "ABC");

            CompactRpcProtocol.WriteCall(originalCall, writerContext);
            networkStream.Position = 0;

            var deserializedCall = CompactRpcProtocol.ReadCall(Resolve<IMethodCallObjectFactory>(), readerContext);

            //-- assert 

            deserializedCall.ShouldNotBeNull();
            deserializedCall.MethodInfo.ShouldBeSameAs(callMethod);
            deserializedCall.GetParameterValue(0).ShouldBe(123);
            deserializedCall.GetParameterValue(1).ShouldBe("ABC");
        }
    }
}

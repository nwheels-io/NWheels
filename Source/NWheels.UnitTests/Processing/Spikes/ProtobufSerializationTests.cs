using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtoBuf;
using ProtoBuf.Meta;

namespace NWheels.UnitTests.Processing.Spikes
{
    [TestFixture]
    public class ProtobufSerializationTests
    {
        [Test]
        public void CanSerialize()
        {
            RuntimeTypeModel.Default.Add(typeof(ClassOne), true);

            var original = new ClassOne() {
                IntValue = 123,
                StringValue = "ABC"
            };

            var stream = new MemoryStream();
            Serializer.NonGeneric.Serialize(stream, original);
            stream.Position = 0;

            Console.WriteLine("stream length={0}", stream.Length);

            var deserialized = (ClassOne)Serializer.NonGeneric.Deserialize(typeof(ClassOne), stream);

            Assert.That(deserialized.IntValue, Is.EqualTo(123));
            Assert.That(deserialized.StringValue, Is.EqualTo("ABC"));
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract]
        public class ClassOne
        {
            [DataMember(Order = 1)]
            public int IntValue { get; set; }
            [DataMember(Order = 2)]
            public string StringValue { get; set; }
        }
    }
}

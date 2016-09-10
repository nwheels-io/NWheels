using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Extensions;
using Shouldly;

namespace NWheels.UnitTests.Extensions
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public void TakeChunks_ExactLength()
        {
            //-- arrange

            var source = new[] { "AA", "BBB", "CCC", "DD" };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(2);
            chunks[0].ShouldBe(new[] { "AA", "BBB" });
            chunks[1].ShouldBe(new[] { "CCC", "DD" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_WithLeftOver()
        {
            //-- arrange

            var source = new[] { "AAA", "BBB", "CC", "DDD" };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(3);
            chunks[0].ShouldBe(new[] { "AAA", });
            chunks[1].ShouldBe(new[] { "BBB", "CC" });
            chunks[2].ShouldBe(new[] { "DDD" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_MultipleLeftovers()
        {
            //-- arrange

            var source = new[] { "AAA", "BBB", "CCC", "DDD" };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(4);
            chunks[0].ShouldBe(new[] { "AAA", });
            chunks[1].ShouldBe(new[] { "BBB" });
            chunks[2].ShouldBe(new[] { "CCC" });
            chunks[3].ShouldBe(new[] { "DDD" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_SingleItemLessThanMaxLength()
        {
            //-- arrange

            var source = new[] { "AAA" };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(1);
            chunks[0].ShouldBe(new[] { "AAA", });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_SingleItemExactlyMaxLength()
        {
            //-- arrange

            var source = new[] { "AAAAA" };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(1);
            chunks[0].ShouldBe(new[] { "AAAAA", });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_SingleItemLongerThanMaxLength()
        {
            //-- arrange

            var source = new[] { "AAABBBCCC" };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(1);
            chunks[0].ShouldBe(new[] { "AAABBBCCC" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_EmptyInput()
        {
            //-- arrange

            var source = new string[0];

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.ShouldNotBeNull();
            chunks.Length.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_NullItems()
        {
            //-- arrange

            var source = new[] { "AAA", null, "BBB", null, "CCC", null, "DDD", null };

            //-- act

            var chunks = source.TakeChunks(maxChunkTextLength: 5).ToArray();

            //-- assert

            chunks.Length.ShouldBe(4);
            chunks[0].ShouldBe(new[] { "AAA", null });
            chunks[1].ShouldBe(new[] { "BBB", null });
            chunks[2].ShouldBe(new[] { "CCC", null });
            chunks[3].ShouldBe(new[] { "DDD", null });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("One", "One")]
        [TestCase("OneTwo", "One two")]
        [TestCase("OneTwoThree", "One two three")]
        [TestCase("HTTP", "HTTP")]
        [TestCase("HTTPProtocol", "HTTP protocol")]
        [TestCase("OneABBR", "One ABBR")]
        [TestCase("OneABBRTwo", "One ABBR two")]
        [TestCase("1234", "1234")]
        [TestCase("One1234", "One 1234")]
        [TestCase("One1234Two", "One 1234 two")]
        [TestCase("OneABBR1234", "One ABBR 1234")]
        [TestCase("OneABBR1234Two", "One ABBR 1234 two")]
        [TestCase("ABBR1", "ABBR 1")]
        [TestCase("One1", "One 1")]
        [TestCase("1ABBR", "1 ABBR")]
        [TestCase("1One", "1 one")]
        [TestCase(" ", " ")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void TestSplitPascalCase(string input, string expectedOutput)
        {
            //-- act

            var actualOutput = input.SplitPascalCase();

            //-- assert

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }
    }
}

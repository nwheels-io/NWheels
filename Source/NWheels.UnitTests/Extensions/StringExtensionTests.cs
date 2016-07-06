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
    }
}

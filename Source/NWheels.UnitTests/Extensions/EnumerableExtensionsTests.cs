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
    public class EnumerableExtensionsTests
    {
        [Test]
        public void TakeChunks_ExactLength()
        {
            //-- arrange

            var input = new[] { 111, 222, 333, 444, 555, 666, 777, 888, 999 };

            //-- act

            var chunks = input.TakeChunks(length: 3).ToArray();

            //-- assert

            chunks.Length.ShouldBe(3);

            chunks[0].ShouldBe(new[] { 111, 222, 333 });
            chunks[1].ShouldBe(new[] { 444, 555, 666 });
            chunks[2].ShouldBe(new[] { 777, 888, 999 });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_NonExactLength()
        {
            //-- arrange

            var input = new[] { 111, 222, 333, 444, 555, 666, 777 };

            //-- act

            var chunks = input.TakeChunks(length: 3).ToArray();

            //-- assert

            chunks.Length.ShouldBe(3);

            chunks[0].ShouldBe(new[] { 111, 222, 333 });
            chunks[1].ShouldBe(new[] { 444, 555, 666 });
            chunks[2].ShouldBe(new[] { 777 });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_SingleChunkExactLength()
        {
            //-- arrange

            var input = new[] { 111, 222, 333 };

            //-- act

            var chunks = input.TakeChunks(length: 3).ToArray();

            //-- assert

            chunks.Length.ShouldBe(1);

            chunks[0].ShouldBe(new[] { 111, 222, 333 });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_SingleChunkNonExactLength()
        {
            //-- arrange

            var input = new[] { 111, 222 };

            //-- act

            var chunks = input.TakeChunks(length: 3).ToArray();

            //-- assert

            chunks.Length.ShouldBe(1);

            chunks[0].ShouldBe(new[] { 111, 222 });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TakeChunks_EmptySource()
        {
            //-- arrange

            var input = new int[0];

            //-- act

            var chunks = input.TakeChunks(length: 3).ToArray();

            //-- assert

            chunks.Length.ShouldBe(0);
        }
    }
}

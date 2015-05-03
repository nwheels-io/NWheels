#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Logging;

namespace NWheels.Testing.UnitTests
{
    [TestFixture]
    public class LogAssertTests : UnitTestBase
    {
        [Test]
        public void Empty_Pass()
        {
            //-- Arrange

            var log = Framework.GetLog();

            //-- Act & Assert

            LogAssert.Empty(log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(AssertionException))]
        public void Empty_Fail()
        {
            //-- Arrange

            Framework.Logger<ILogger>().One("ABC", 123);
            var log = Framework.GetLog();

            //-- Act & Assert

            LogAssert.Empty(log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanMatchByIdAndAnyValues()
        {
            //-- Arrange

            var logger = Framework.Logger<ILogger>();

            logger.One("ABC", 123);
            logger.Two(Guid.NewGuid(), TimeSpan.FromDays(1));
            logger.One("DEF", 456);
            logger.Two(Guid.NewGuid(), TimeSpan.FromDays(2));

            var log = Framework.GetLog();

            //-- Act 

            var matched = log.Where(LogAssert.Match<ILogger>(x => x.One(LogIs.Any<string>(), LogIs.Any<int>()))).ToArray();

            //-- Assert

            Assert.That(matched.Length, Is.EqualTo(2));
            Assert.That(matched[0].SingleLineText, Is.EqualTo("One: str=ABC, num=123"));
            Assert.That(matched[1].SingleLineText, Is.EqualTo("One: str=DEF, num=456"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanMatchByIdAndExactValues()
        {
            //-- Arrange

            var logger = Framework.Logger<ILogger>();

            logger.One("ZZZ", 999);
            logger.One("ABC", 123);
            logger.Two(Guid.NewGuid(), TimeSpan.FromDays(1));
            logger.One("DEF", 456);
            logger.Two(Guid.NewGuid(), TimeSpan.FromDays(2));

            var log = Framework.GetLog();

            //-- Act 

            var matched = log.Where(LogAssert.Match<ILogger>(x => x.One("ABC", 123))).ToArray();

            //-- Assert

            Assert.That(matched.Length, Is.EqualTo(1));
            Assert.That(matched[0].SingleLineText, Is.EqualTo("One: str=ABC, num=123"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ExpectOnce_Pass()
        {
            //-- Arrange

            var logger = Framework.Logger<ILogger>();

            logger.One("ABC", 123);
            logger.One("DEF", 456);

            var log = Framework.GetLog();

            //-- Act & Assert

            LogAssert
                .ExpectOnce<ILogger>(x => x.One("ABC", 123))
                .Verify(log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(AssertionException), ExpectedMessage = "One(str=ABC, num=123)", MatchType = MessageMatch.Contains)]
        public void ExpectOnce_Fail()
        {
            //-- Arrange

            var logger = Framework.Logger<ILogger>();

            logger.One("ZZZ", 999);
            logger.One("VVV", 888);

            var log = Framework.GetLog();

            //-- Act & Assert

            LogEx
                .One.Match<ILogger>(x => x.One("ABC", 123))
                .ZeroOrMore.NotEqualTo<ILogger>(x => x.One("ABC", LogEx.IsAny<int>()))
                .OneOrMore<ILogger>(x => x.One("ABC", 123))
                .None<ILogger>(x => x.One("ABC", 123))
                .AtMost<ILogger>(x => x.One("ABC", 123))
                .Then.MatchOne<ILogger>(x => x.One("ABC", 123))
                .MatchOne<ILogger>(x => x.One("ABC", 123))
                .Verify(log);


        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogDebug]
            void One(string str, int num);
            [LogDebug]
            void Two(Guid id, TimeSpan time);
        }
    }
}

#endif
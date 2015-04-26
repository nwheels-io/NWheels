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
        public void LogAssertEmpty_Pass()
        {
            //-- Arrange

            var log = Framework.GetLog();

            //-- Act & Assert

            LogAssert.Empty(log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(AssertionException))]
        public void LogAssertEmpty_Fail()
        {
            //-- Arrange

            Framework.Logger<ILogger>().One("ABC", 123);
            var log = Framework.GetLog();

            //-- Act & Assert

            LogAssert.Empty(log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void LogAssertMatchById()
        {
            //-- Arrange

            var logger = Framework.Logger<ILogger>();
            
            logger.One("ABC", 123);
            logger.Two(Guid.NewGuid(), TimeSpan.FromDays(1));
            logger.One("DEF", 456);
            logger.Two(Guid.NewGuid(), TimeSpan.FromDays(2));

            var log = Framework.GetLog();

            //-- Act 

            var matched = log.Where(LogAssert.MatchId<ILogger>(x => x.One(null, 0))).ToArray();

            //-- Assert

            Assert.That(matched.Length, Is.EqualTo(2));
            Assert.That(matched[0].SingleLineText, Is.EqualTo("One: str=ABC, num=123"));
            Assert.That(matched[1].SingleLineText, Is.EqualTo("One: str=DEF, num=456"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Testing.Logging.Core;
using NWheels.Testing.Logging.Impl;

namespace NWheels.Testing.UnitTests
{
    [TestFixture]
    public class LogexTests : UnitTestBase
    {
        [Test]
        public void TestAnyNodeMatcher()
        {
            //-- Arrange

            var node1 = new NameValuePairLogNode("msg1", LogLevel.Debug, exception: null);
            var node2 = new NameValuePairLogNode("msg2", LogLevel.Error, exception: new Exception("e1"));
            
            var matcher = new LogexImpl.AnyNodeMatcher();

            //-- Act 

            var match1 = matcher.Match(node1);
            var match2 = matcher.Match(node2);

            //-- Assert

            Assert.IsTrue(match1);
            Assert.IsTrue(match2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByLevelNodeMatcher()
        {
            //-- Arrange

            var node1 = new NameValuePairLogNode("w1", LogLevel.Warning, exception: null);
            var node2 = new NameValuePairLogNode("e2", LogLevel.Error, exception: new Exception("e2"));

            var matcher = new LogexImpl.ByLevelNodeMatcher(new[] { LogLevel.Warning });

            //-- Act 

            var match1 = matcher.Match(node1);
            var match2 = matcher.Match(node2);

            //-- Assert

            Assert.IsTrue(match1);
            Assert.IsFalse(match2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByLevelOrHigherNodeMatcher()
        {
            //-- Arrange

            var node1 = new NameValuePairLogNode("w1", LogLevel.Warning, exception: null);
            var node2 = new NameValuePairLogNode("e2", LogLevel.Error, exception: new Exception("e2"));
            var node3 = new NameValuePairLogNode("v3", LogLevel.Verbose, exception: null);

            var matcher = new LogexImpl.ByLevelOrHigherNodeMatcher(LogLevel.Warning);

            //-- Act 

            var match1 = matcher.Match(node1);
            var match2 = matcher.Match(node2);
            var match3 = matcher.Match(node3);

            //-- Assert

            Assert.IsTrue(match1);
            Assert.IsTrue(match2);
            Assert.IsFalse(match3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestLByLevelOrLowerNodeMatcher()
        {
            //-- Arrange

            var node1 = new NameValuePairLogNode("w1", LogLevel.Warning, exception: null);
            var node2 = new NameValuePairLogNode("e2", LogLevel.Error, exception: new Exception("e2"));
            var node3 = new NameValuePairLogNode("v3", LogLevel.Verbose, exception: null);

            var matcher = new LogexImpl.ByLevelOrLowerNodeMatcher(LogLevel.Warning);

            //-- Act 

            var match1 = matcher.Match(node1);
            var match2 = matcher.Match(node2);
            var match3 = matcher.Match(node3);

            //-- Assert

            Assert.IsTrue(match1);
            Assert.IsFalse(match2);
            Assert.IsTrue(match3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByLoggerNodeMatcher()
        {
            //-- Arrange

            var logger1 = Framework.Logger<ILogger>();
            var logger2 = Framework.Logger<IAnotherLogger>();

            logger1.One("ABC", 123);
            logger2.Three("DEF", 456);
            logger1.Two(Guid.Empty, TimeSpan.Zero);
            logger2.Four(Guid.Empty, TimeSpan.Zero);

            var matcher = new LogexImpl.ByLoggerNodeMatcher(typeof(ILogger));
            var log = Framework.GetLog();

            //-- Act 

            var matchResults = log.Select(node => matcher.Match(node)).ToArray();

            //-- Assert

            Assert.That(matchResults, Is.EqualTo(new[] { true, false, true, false }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByMessageNodeMatcher_NoParameters()
        {
            //-- Arrange

            Expression<Action<ILogger>> messageSpecifier = (x => x.Zero());
            var matcher = new LogexImpl.ByMessageNodeMatcher(messageSpecifier);

            var logger = Framework.Logger<ILogger>();
            logger.One("ABC", 123);
            logger.Zero();
            logger.Two(Guid.Empty, TimeSpan.Zero);
            var log = Framework.GetLog();

            //-- Act 

            var matchResults = log.Select(node => matcher.Match(node)).ToArray();

            //-- Assert

            Assert.That(matchResults, Is.EqualTo(new[] { false, true, false }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByMessageNodeMatcher_ExactParameterValues()
        {
            //-- Arrange

            Expression<Action<ILogger>> messageSpecifier = (x => x.One("DEF", 456));
            var matcher = new LogexImpl.ByMessageNodeMatcher(messageSpecifier);

            var logger = Framework.Logger<ILogger>();
            logger.Two(Guid.Empty, TimeSpan.Zero);
            logger.One("ABC", 123);
            logger.One("DEF", 456);
            logger.Zero();
            var log = Framework.GetLog();

            //-- Act 

            var matchResults = log.Select(node => matcher.Match(node)).ToArray();

            //-- Assert

            Assert.That(matchResults, Is.EqualTo(new[] { false, false, true, false }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByMessageNodeMatcher_AnyParameterValues()
        {
            //-- Arrange

            Expression<Action<ILogger>> messageSpecifier = (x => x.One(Logex.Any<string>(), Logex.Any<int>()));
            var matcher = new LogexImpl.ByMessageNodeMatcher(messageSpecifier);

            var logger = Framework.Logger<ILogger>();
            logger.Two(Guid.Empty, TimeSpan.Zero);
            logger.One("ABC", 123);
            logger.One("DEF", 456);
            logger.Zero();
            var log = Framework.GetLog();

            //-- Act 

            var matchResults = log.Select(node => matcher.Match(node)).ToArray();

            //-- Assert

            Assert.That(matchResults, Is.EqualTo(new[] { false, true, true, false }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestByMessageNodeMatcher_MixedExactAndAnyParameterValues()
        {
            //-- Arrange

            var guid1 = new Guid("D6B668D5-190D-47D6-9404-700DBDD77A97");
            var guid2 = new Guid("A802C478-15F1-46F4-A70E-864BC65F6926");
            
            Expression<Action<ILogger>> messageSpecifier1 = (x => x.One(Logex.Any<string>(), 123));
            Expression<Action<ILogger>> messageSpecifier2 = (x => x.Two(guid1, Logex.Any<TimeSpan>()));
            var matcher1 = new LogexImpl.ByMessageNodeMatcher(messageSpecifier1);
            var matcher2 = new LogexImpl.ByMessageNodeMatcher(messageSpecifier2);

            var logger = Framework.Logger<ILogger>();
            logger.One("ABC", 123);
            logger.One("ABC", 456);
            logger.One("DEF", 123);
            logger.One("DEF", 456);
            logger.Two(guid1, TimeSpan.Zero);
            logger.Two(guid2, TimeSpan.Zero);
            logger.Two(guid1, TimeSpan.FromDays(1));
            logger.Two(guid2, TimeSpan.FromDays(1));
            var log = Framework.GetLog();

            //-- Act 

            var matchResults1 = log.Select(node => matcher1.Match(node)).ToArray();
            var matchResults2 = log.Select(node => matcher2.Match(node)).ToArray();

            //-- Assert

            Assert.That(matchResults1, Is.EqualTo(new[] { true, false, true, false, false, false, false, false }));
            Assert.That(matchResults2, Is.EqualTo(new[] { false, false, false, false, true, false, true, false }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, true)]
        public void TestOperatorAndMatcher(bool left, bool right, bool expectedResult)
        {
            //-- Arrange

            var matchLog = new List<LogNode>();
            var matcher = new LogexImpl.OperatorAndNodeMatcher(new TestNodeMatcher(matchLog, left), new TestNodeMatcher(matchLog, right));
            var node = new NameValuePairLogNode("m1", LogLevel.Debug, exception: null);

            //-- Act 

            var actualResult = matcher.Match(node);
            
            //-- Assert

            Assert.That(actualResult, Is.EqualTo(expectedResult));
            Assert.That(matchLog.Count, Is.GreaterThan(0));
            matchLog.ForEach(loggedNode => Assert.That(loggedNode, Is.SameAs(node)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase(false, false, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        public void TestOperatorOrMatcher(bool left, bool right, bool expectedResult)
        {
            //-- Arrange

            var matchLog = new List<LogNode>();
            var matcher = new LogexImpl.OperatorOrNodeMatcher(new TestNodeMatcher(matchLog, left), new TestNodeMatcher(matchLog, right));
            var node = new NameValuePairLogNode("m1", LogLevel.Debug, exception: null);

            //-- Act 

            var actualResult = matcher.Match(node);

            //-- Assert

            Assert.That(actualResult, Is.EqualTo(expectedResult));
            Assert.That(matchLog.Count, Is.GreaterThan(0));
            matchLog.ForEach(loggedNode => Assert.That(loggedNode, Is.SameAs(node)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void TestOperatorNotMatcher(bool operand, bool expectedResult)
        {
            //-- Arrange

            var matchLog = new List<LogNode>();
            var matcher = new LogexImpl.OperatorNotNodeMatcher(new TestNodeMatcher(matchLog, operand));
            var node = new NameValuePairLogNode("m1", LogLevel.Debug, exception: null);

            //-- Act 

            var actualResult = matcher.Match(node);

            //-- Assert

            Assert.That(actualResult, Is.EqualTo(expectedResult));
            Assert.That(matchLog.Count, Is.EqualTo(1));
            Assert.That(matchLog[0], Is.SameAs(node));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("A1", true, null)]
        [TestCase("A1;A2/123", true, null)]
        [TestCase("A2/123;A1", false, "[0] expected: 'message LogexTests.LoggerA.A1()' but was: 'Verbose|LogexTests.LoggerA.A2(x=123)'")]
        [TestCase("A2/123", false, "[0] expected: 'message LogexTests.LoggerA.A1()' but was: 'Verbose|LogexTests.LoggerA.A2(x=123)'")]
        public void TestLogex_SigleOneTime(string input, bool expectedMatchResult, string expectedMismatchDescription)
        {
            //-- arrange

            var log = LogTestHelper.ParseLogFromString(input, Framework);
            var logex = Logex.Begin().One().Message<ILoggerA>(a => a.A1()).End();
            
            //-- act

            string mismatchDescription;
            var matchResult = logex.Match(log, out mismatchDescription);

            //-- assert

            Assert.That(matchResult, Is.EqualTo(expectedMatchResult));
            Assert.That(mismatchDescription, Is.EqualTo(expectedMismatchDescription));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("A1;A2/123;A2/456", true, null)]
        [TestCase("A1;A2/123;A2/999", false, "[2] expected: 'message LogexTests.LoggerA.A2(x=456)' but was: 'Verbose|LogexTests.LoggerA.A2(x=999)'")]
        [TestCase("A1;A2/123", false, "end of log: expected 'message LogexTests.LoggerA.A2(x=456)'")]
        [TestCase("", false, "end of log: expected 'message LogexTests.LoggerA.A1()'")]
        public void TestLogex_MultipleOneTimeConcatenated(string input, bool expectedMatchResult, string expectedMismatchDescription)
        {
            //-- arrange

            var log = LogTestHelper.ParseLogFromString(input, Framework);
            var logex = Logex
                .Begin()
                    .One().Message<ILoggerA>(a => a.A1())
                    .One().Message<ILoggerA>(a => a.A2(123))
                    .One().Message<ILoggerA>(a => a.A2(456))
                .End();

            //-- act

            string mismatchDescription;
            var matchResult = logex.Match(log, out mismatchDescription);

            //-- assert

            Assert.That(matchResult, Is.EqualTo(expectedMatchResult));
            Assert.That(mismatchDescription, Is.EqualTo(expectedMismatchDescription));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("B1;A1;A2/123", true, null)]
        [TestCase("B1;A2/123;A1", true, null)]
        [TestCase("A2/123;A1;B1", true, null)]
        [TestCase("A2/123", true, null)]
        [TestCase("A1;B1;A1;B1", false, "end of log: expected 'message LogexTests.LoggerA.A2(x=123)'")]
        [TestCase("", false, "end of log: expected 'message LogexTests.LoggerA.A2(x=123)'")]
        public void TestLogex_OneTimePaddedWithZeroOrMore(string input, bool expectedMatchResult, string expectedMismatchDescription)
        {
            //-- arrange

            var log = LogTestHelper.ParseLogFromString(input, Framework);
            var logex = Logex
                .Begin()
                    .ZeroOrMore().AnyMessage()
                    .One().Message<ILoggerA>(a => a.A2(123))
                    .ZeroOrMore().AnyMessage()
                .End();

            //-- act

            string mismatchDescription;
            var matchResult = logex.Match(log, out mismatchDescription);

            //-- assert

            Assert.That(matchResult, Is.EqualTo(expectedMatchResult));
            Assert.That(mismatchDescription, Is.EqualTo(expectedMismatchDescription));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("A1;B1", true, null)]
        [TestCase("A1;A2/123;B1", true, null)]
        [TestCase("C1;C1;A1;B1", true, null)]
        [TestCase("A1;B1;C1;C1", true, null)]
        [TestCase("C1;B1", false, "end of log: expected 'from logger NWheels.Testing.UnitTests.LogexTests+ILoggerA'")]
        [TestCase("", false, "end of log: expected 'from logger NWheels.Testing.UnitTests.LogexTests+ILoggerA'")]
        public void TestLogex_ZeroOrMoreCombinedWithOneOrMoreThenOne(string input, bool expectedMatchResult, string expectedMismatchDescription)
        {
            //-- arrange

            var log = LogTestHelper.ParseLogFromString(input, Framework);
            var logex = Logex
                .Begin()
                    .ZeroOrMore().AnyMessage()
                    .OneOrMore().From<ILoggerA>()
                    .One().From<ILoggerB>()
                .End();

            //-- act

            string mismatchDescription;
            var matchResult = logex.Match(log, out mismatchDescription);

            //-- assert

            Assert.That(matchResult, Is.EqualTo(expectedMatchResult));
            Assert.That(mismatchDescription, Is.EqualTo(expectedMismatchDescription));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("A1;A2/123;B1;B2/123", true, null)]
        [TestCase("A1;B1;B2/123", true, null)]
        [TestCase("A1;B1", true, null)]
        [TestCase("A1;A2/123;B1;B2/123", true, null)]
        [TestCase("A1;B1;B2/123", true, null)]
        [TestCase("A1;A2/123", false, "end of log: expected 'from logger NWheels.Testing.UnitTests.LogexTests+ILoggerB'")]
        [TestCase("A1;B1;A1", false, "[2] expected: 'end of log' but was: 'Debug|LogexTests.LoggerA.A1()'")]
        public void TestLogex_EndOfLog(string input, bool expectedMatchResult, string expectedMismatchDescription)
        {
            //-- arrange

            var log = LogTestHelper.ParseLogFromString(input, Framework);
            var logex = Logex.Begin()
                .OneOrMore().From<ILoggerA>()
                .OneOrMore().From<ILoggerB>()
                .EndOfLog();

            //-- act

            string mismatchDescription;
            var matchResult = logex.Match(log, out mismatchDescription);

            //-- assert

            Assert.That(matchResult, Is.EqualTo(expectedMatchResult));
            Assert.That(mismatchDescription, Is.EqualTo(expectedMismatchDescription));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogDebug]
            void Zero();
            [LogDebug]
            void One(string str, int num);
            [LogDebug]
            void Two(Guid id, TimeSpan time);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAnotherLogger : IApplicationEventLogger
        {
            [LogDebug]
            void Three(string str, int num);
            [LogDebug]
            void Four(Guid id, TimeSpan time);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILoggerA : IApplicationEventLogger
        {
            [LogDebug]
            void A1();
            [LogVerbose]
            void A2(int x);
            [LogInfo]
            void A3(int x, int y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILoggerB : IApplicationEventLogger
        {
            [LogWarning]
            void B1();
            [LogError]
            void B2(int z);
            [LogCritical]
            void B3(int z, int w);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILoggerC : IApplicationEventLogger
        {
            [LogVerbose]
            void C1();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static class LogTestHelper
        {
            public static LogNode[] ParseLogFromString(string s, TestFramework framework)
            {
                var nodeStrings = s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach ( var node in nodeStrings )
                {
                    WriteNode(node, framework);
                }

                return framework.TakeLog();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void WriteNode(string nodeString, TestFramework framework)
            {
                var messageId = (nodeString.Substring(0, 2));
                var values = ( 
                    nodeString.Length > 3 ? 
                    nodeString.Substring(3).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray() :
                    new int[0]);

                if ( messageId == "A1" )
                {
                    framework.Logger<ILoggerA>().A1();
                }
                else if ( messageId == "A2" )
                {
                    framework.Logger<ILoggerA>().A2(values[0]);
                }
                else if ( messageId == "A3" )
                {
                    framework.Logger<ILoggerA>().A3(values[0], values[1]);
                }
                else if ( messageId == "B1" )
                {
                    framework.Logger<ILoggerB>().B1();
                }
                else if ( messageId == "B2" )
                {
                    framework.Logger<ILoggerB>().B2(values[0]);
                }
                else if ( messageId == "B3" )
                {
                    framework.Logger<ILoggerB>().B3(values[0], values[1]);
                }
                else if ( messageId == "C1" )
                {
                    framework.Logger<ILoggerC>().C1();
                }
                else
                {
                    throw new ArgumentException("Invalid log string");
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestNodeMatcher : ILogexNodeMatcher
        {
            private readonly List<LogNode> _matchLog;
            private readonly bool _willMatch;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestNodeMatcher(List<LogNode> matchLog, bool willMatch)
            {
                _matchLog = matchLog;
                _willMatch = willMatch;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public bool Match(LogNode node)
            {
                _matchLog.Add(node);                
                return _willMatch;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public string Describe()
            {
                return null;
            }
        }
    }
}

#if false

{1,1}==MSG1         
{0,M}==*			
{0,3}==MSG2
{1,M}!=MSG3
{1,1}==MSG3

(MSG1).*(MSG2){0,3}[^(MSG3)]+$


.One.Message<IMyLogger>(x => x.MSG1)
.ZeroOrMore.AnyMessage()
.AtMost(3).Message<IMyLogger>(x => x.MSG2)
.One.NotMessage<IMyLogger>(x => x.MSG2)
.AllToEnd.NotMessage<IMyLogger>(x => x.MSG3)

Occurrence Multiplicity Operators: 
	
	Between(m,n)    - [m,n]
	One             - [1,1]
	OneOrMore       - [1,*)
	ZeroOrOne       - [0,1]
	ZeroOrMore      - [0,*)
	AtLeasn(n)      - [n,*)
	AtMost(n)       - [0,n]
	AllToEnd        - 
	NoneToEnd
	
Match Operators:

	AnyMessage()
	AtLevel(Log-levels)
	WarningOrError
	From<TLogger>()
	Message<TLogger>(Message + Parameter Value Matchers)
	
	NotAtLevel(Log-levels)
	NotWarningOrError
	NotFrom<TLogger>()
	NotMessage<TLogger>(Message + Parameter Value Matchers)

Composite Match Operators:	
	
	AnyOf(match-operators)
	AllOf(match-operators)
	AndAnyOf(match-operators)
	AndAllOf(match-operators)
	OrAnyOf(match-operators)
	OrAllOf(match-operators)
	
#endif

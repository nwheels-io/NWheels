using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Testing.Logging.Impl;

namespace NWheels.Testing
{
    public static class LogAssert
    {
        public static Assertions That(IEnumerable<LogNode> log)
        {
            return new Assertions(log);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<LogNode> From<TLogger>(this IEnumerable<LogNode> log) 
            where TLogger : IApplicationEventLogger
        {
            var matcher = new LogexImpl.ByLoggerNodeMatcher(typeof(TLogger));
            return log.Where(matcher.Match);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Assertions
        {
            private readonly IEnumerable<LogNode> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Assertions(IEnumerable<LogNode> log)
            {
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Matches(Logex logex)
            {
                string mismatch;
                var matched = logex.Match(_log, out mismatch);

                if ( !matched )
                {
                    Assert.Fail("Log assertion failed: " + mismatch);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasOne<TLogger>(Expression<Action<TLogger>> messageSpecifier)
                where TLogger : class, IApplicationEventLogger
            {
                Matches(Logex.Begin().One().Message<TLogger>(messageSpecifier).End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasOneOrMore<TLogger>(Expression<Action<TLogger>> messageSpecifier)
                where TLogger : class, IApplicationEventLogger
            {
                Matches(Logex.Begin().OneOrMore().Message<TLogger>(messageSpecifier).End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasNone<TLogger>(Expression<Action<TLogger>> messageSpecifier)
                where TLogger : class, IApplicationEventLogger
            {
                Matches(Logex.Begin().NoneToEnd().Message<TLogger>(messageSpecifier).End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasMessagesFrom<TLogger>()
                where TLogger : class, IApplicationEventLogger
            {
                Matches(Logex.Begin().OneOrMore().From<TLogger>().End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasNoMessagesFrom<TLogger>()
                where TLogger : class, IApplicationEventLogger
            {
                Matches(Logex.Begin().NoneToEnd().From<TLogger>().End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasNoErrors()
            {
                Matches(Logex.Begin().NoneToEnd().OfLevelOrHigher(LogLevel.Error).End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasNoWarnings()
            {
                Matches(Logex.Begin().NoneToEnd().OfLevel(LogLevel.Warning).End());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void HasNoErrorsOrWarnings()
            {
                Matches(Logex.Begin().NoneToEnd().WarningOrError().End());
            }
        }


        #if false
        public static void Empty(IEnumerable<LogNode> log)
        {
            var logArray = log.ToArray();

            if ( logArray.Length > 0 )
            {
                Assert.Fail("Expected no log messages, but was: {0}", logArray.Length);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Func<LogNode, bool> Match<TLogger>(Expression<Action<TLogger>> messageSelector)
        {
            var matcher = new NodeMatcher(messageSelector);
            return matcher.Match;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IValueMatcher CreateValueMatcher(Expression expectedValueExpression)
        {
            var constant = (expectedValueExpression as ConstantExpression);

            if ( constant != null )
            {
                return new ExactValueMatcher(constant.Value);
            }
            else
            {
                return new AnyValueMatcher();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IExpectationBuilder
        {
            IExpectationBuilder ExpectNone(params LogLevel[] levels);
            IExpectationBuilder ExpectOnce(params LogLevel[] levels);
            IExpectationBuilder ExpectAtLeast(int numberOfTimes, params LogLevel[] levels);
            IExpectationBuilder ExpectAtMost(int numberOfTimes, params LogLevel[] levels);
            IExpectationBuilder ExpectBetween(int fromNumberOfTimes, int toNumberOfTimes, params LogLevel[] levels);
            IExpectationBuilder ExpectExactly(int numberOfTimes, params LogLevel[] levels);
            IExpectationBuilder ExpectNone<TLogger>(Expression<Action<TLogger>> messageSpecifier);
            IExpectationBuilder ExpectOnce<TLogger>(Expression<Action<TLogger>> messageSpecifier);
            IExpectationBuilder ExpectAtLeast<TLogger>(int numberOfTimes, Expression<Action<TLogger>> messageSpecifier);
            IExpectationBuilder ExpectAtMost<TLogger>(int numberOfTimes, Expression<Action<TLogger>> messageSpecifier);
            IExpectationBuilder ExpectBetween<TLogger>(int fromNumberOfTimes, int toNumberOfTimes, Expression<Action<TLogger>> messageSpecifier);
            IExpectationBuilder ExpectExactly<TLogger>(int numberOfTimes, Expression<Action<TLogger>> messageSpecifier);
            IExpectationBuilder Then();
            void Verify(IEnumerable<LogNode> log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface INodeMatcher
        {
            bool Match(LogNode actualNode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IValueMatcher
        {
            bool Match(object actualValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface IExpectation
        {
            bool Verify(IEnumerable<LogNode> log);
            string Describe();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ExpectationSequence : IExpectation, IExpectationBuilder
        {

            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectNone(params LogLevel[] levels)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectOnce(params LogLevel[] levels)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectAtLeast(int numberOfTimes, params LogLevel[] levels)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectAtMost(int numberOfTimes, params LogLevel[] levels)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectBetween(int fromNumberOfTimes, int toNumberOfTimes, params LogLevel[] levels)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectExactly(int numberOfTimes, params LogLevel[] levels)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectNone<TLogger>(Expression<Action<TLogger>> messageSpecifier)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectOnce<TLogger>(Expression<Action<TLogger>> messageSpecifier)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectAtLeast<TLogger>(int numberOfTimes, Expression<Action<TLogger>> messageSpecifier)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IExpectationBuilder IExpectationBuilder.ExpectAtMost<TLogger>(int numberOfTimes, Expression<Action<TLogger>> messageSpecifier)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            IExpectationBuilder IExpectationBuilder.ExpectBetween<TLogger>(int fromNumberOfTimes, int toNumberOfTimes, Expression<Action<TLogger>> messageSpecifier)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            IExpectationBuilder IExpectationBuilder.ExpectExactly<TLogger>(int numberOfTimes, Expression<Action<TLogger>> messageSpecifier)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            IExpectationBuilder IExpectationBuilder.Then()
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IExpectationBuilder.Verify(IEnumerable<LogNode> log)
            {
                throw new NotImplementedException();
            }

            bool IExpectation.Verify(IEnumerable<LogNode> log)
            {
                throw new NotImplementedException();
            }

            string IExpectation.Describe()
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NodeMatcher : INodeMatcher
        {
            private readonly string _messageId;
            private readonly string[] _argumentNames;
            private readonly IValueMatcher[] _argumentValueMatchers;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeMatcher(LambdaExpression messageSelector)
            {
                Expression[] values;
                var method = messageSelector.GetMethodInfo(out values);
                
                var parameters = method.GetParameters();

                if ( parameters.Length != values.Length )
                {
                    throw new NotSupportedException("Specified expectatiion is not supported.");
                }

                _messageId = ApplicationEventLoggerConvention.GetMessageId(method);
                _argumentNames = parameters.Select(p => p.Name).ToArray();
                _argumentValueMatchers = values.Select(CreateValueMatcher).ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode actualNode)
            {
                if ( actualNode.MessageId != _messageId )
                {
                    return false;
                }

                var allPairs = actualNode.NameValuePairs;

                for ( int i = 0 ; i < _argumentNames.Length ; i++ )
                {
                    var pair = allPairs.FirstOrDefault(p => p.FormatName() == _argumentNames[i]);

                    if ( pair == null )
                    {
                        return false;
                    }

                    if ( !_argumentValueMatchers[i].Match(pair.GetValueAsObject()) )
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ExactValueMatcher : IValueMatcher
        {
            private readonly object _expectedValue;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExactValueMatcher(object expectedValue)
            {
                _expectedValue = expectedValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(object actualValue)
            {
                if ( object.ReferenceEquals(actualValue, _expectedValue) )
                {
                    return true;
                }

                if ( actualValue != null && _expectedValue != null )
                {
                    return actualValue.Equals(_expectedValue);
                }

                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AnyValueMatcher : IValueMatcher
        {
            public bool Match(object actualValue)
            {
                return true;
            }
        }

        #endif
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NWheels.Logging;
using System.Linq.Expressions;
using NWheels.Testing.Logging.Core;
using NWheels.Testing.Logging.Impl;

namespace NWheels.Testing
{
    public class Logex
    {
        private readonly LogexSegment[] _segments;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Logex(LogexSegment[] segments)
        {
            _segments = segments;

            for ( int i = 1 ; i < _segments.Length ; i++ )
            {
                _segments[i - 1].NextSegment = _segments[i];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Match(IEnumerable<LogNode> log, out string mismatchDescription)
        {
            var input = log.ToArray();

            if ( _segments.Length > 0 )
            {
                return _segments[0].Match(input, inputIndex: 0, mismatchDescription: out mismatchDescription);
            }
            else
            {
                mismatchDescription = null;
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ILogexBuilder Begin()
        {
            return new LogexBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Any<T>()
        {
            return default(T);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T In<T>(params T[] valueList)
        {
            return default(T);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogexBuilder
        {
            Logex End();
            Logex EndOfLog();
            ILogexMatchBuilder Between(int fromTimes, int toTimes);
            ILogexMatchBuilder Exactly(int times);
            ILogexMatchBuilder One();
            ILogexMatchBuilder OneOrMore();
            ILogexMatchBuilder ZeroOrOne();
            ILogexMatchBuilder ZeroOrMore();
            ILogexMatchBuilder AtLeast(int times);
            ILogexMatchBuilder AtMost(int times);
            ILogexMatchBuilder AllToEnd();
            ILogexMatchBuilder NoneToEnd();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogexMatchBuilder
        {
            ILogexBuilder AnyMessage();
            ILogexBuilder OfLevel(params LogLevel[] levels);
            ILogexBuilder OfLevelOrHigher(LogLevel level);
            ILogexBuilder OfLevelOrLower(LogLevel level);
            ILogexBuilder WarningOrError();
            ILogexBuilder From<TLogger>()
                where TLogger : class, IApplicationEventLogger;
            ILogexBuilder Message<TLogger>(Expression<Action<TLogger>> messageSpecifier)
                where TLogger : class, IApplicationEventLogger;
            ILogexBuilder NotOfLevel(params LogLevel[] levels);
            ILogexBuilder NotOfLevelOrHigher(LogLevel level);
            ILogexBuilder NotOfLevelOrLower(LogLevel level);
            ILogexBuilder NotWarningOrError();
            ILogexBuilder NotFrom<TLogger>()
                where TLogger : class, IApplicationEventLogger;
            ILogexBuilder NotMessage<TLogger>(Expression<Action<TLogger>> messageSpecifier)
                where TLogger : class, IApplicationEventLogger;
            ILogexBuilder AllOf(params Func<ILogexMatchBuilder, ILogexBuilder>[] andedMatches);
            ILogexBuilder AnyOf(params Func<ILogexMatchBuilder, ILogexBuilder>[] oredMatches);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LogexBuilder : ILogexBuilder, ILogexMatchBuilder
        {
            private readonly List<LogexSegment> _segments;
            private readonly Stack<List<ILogexNodeMatcher>> _nestedMatchers;
            private ILogexTemplate _template;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LogexBuilder()
            {
                _segments = new List<LogexSegment>();
                _nestedMatchers = new Stack<List<ILogexNodeMatcher>>();
                _template = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex End()
            {
                return new Logex(_segments.ToArray());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex EndOfLog()
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(1, 1, "1 time"));
                PushMatcher(new LogexImpl.EndOfLogMatcher());
                return End();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder Between(int fromTimes, int toTimes)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder Exactly(int times)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder One()
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(1, 1, "1 time"));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder OneOrMore()
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(1, Int32.MaxValue, "1+ times"));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder ZeroOrOne()
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(0, 1));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder ZeroOrMore()
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(0, Int32.MaxValue, "0+ times"));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder AtLeast(int times)
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(times, Int32.MaxValue, times + "+ times"));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder AtMost(int times)
            {
                PushMultiplier(new LogexImpl.RangeOfTimesMultiplier(0, times));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder AllToEnd()
            {
                _template = new LogexImpl.AllToEndTemplate();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Logex.ILogexMatchBuilder NoneToEnd()
            {
                _template = new LogexImpl.NoneToEndTemplate();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder AnyMessage()
            {
                PushMatcher(new LogexImpl.AnyNodeMatcher());
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder OfLevel(params LogLevel[] levels)
            {
                PushMatcher(new LogexImpl.ByLevelNodeMatcher(levels));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder OfLevelOrHigher(LogLevel level)
            {
                PushMatcher(new LogexImpl.ByLevelOrHigherNodeMatcher(level));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder OfLevelOrLower(LogLevel level)
            {
                PushMatcher(new LogexImpl.ByLevelOrLowerNodeMatcher(level));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder WarningOrError()
            {
                PushMatcher(new LogexImpl.ByLevelOrHigherNodeMatcher(LogLevel.Warning));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder From<TLogger>() where TLogger : class, IApplicationEventLogger
            {
                PushMatcher(new LogexImpl.ByLoggerNodeMatcher(typeof(TLogger)));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder Message<TLogger>(Expression<Action<TLogger>> messageSpecifier) where TLogger : class, IApplicationEventLogger
            {
                PushMatcher(new LogexImpl.ByMessageNodeMatcher(messageSpecifier));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder NotOfLevel(params LogLevel[] levels)
            {
                PushMatcher(new LogexImpl.OperatorNotNodeMatcher(new LogexImpl.ByLevelNodeMatcher(levels)));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder NotOfLevelOrHigher(LogLevel level)
            {
                PushMatcher(new LogexImpl.OperatorNotNodeMatcher(new LogexImpl.ByLevelOrHigherNodeMatcher(level)));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder NotOfLevelOrLower(LogLevel level)
            {
                PushMatcher(new LogexImpl.OperatorNotNodeMatcher(new LogexImpl.ByLevelOrLowerNodeMatcher(level)));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder NotWarningOrError()
            {
                PushMatcher(new LogexImpl.OperatorNotNodeMatcher(new LogexImpl.ByLevelOrHigherNodeMatcher(LogLevel.Warning)));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder NotFrom<TLogger>() where TLogger : class, IApplicationEventLogger
            {
                PushMatcher(new LogexImpl.OperatorNotNodeMatcher(new LogexImpl.ByLoggerNodeMatcher(typeof(TLogger))));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder NotMessage<TLogger>(Expression<Action<TLogger>> messageSpecifier) where TLogger : class, IApplicationEventLogger
            {
                PushMatcher(new LogexImpl.OperatorNotNodeMatcher(new LogexImpl.ByMessageNodeMatcher(messageSpecifier)));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder AllOf(params Func<Logex.ILogexMatchBuilder, ILogexBuilder>[] andedMatches)
            {
                PushCompositeMatcher(operands => new LogexImpl.OperatorAndNodeMatcher(operands), andedMatches);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ILogexBuilder AnyOf(params Func<Logex.ILogexMatchBuilder, ILogexBuilder>[] oredMatches)
            {
                PushCompositeMatcher(operands => new LogexImpl.OperatorOrNodeMatcher(operands), oredMatches);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PushCompositeMatcher(
                Func<ILogexNodeMatcher[], ILogexNodeMatcher> compositeFactory,
                Func<Logex.ILogexMatchBuilder, ILogexBuilder>[] nestedFactories)
            {
                var operands = new List<ILogexNodeMatcher>();
                _nestedMatchers.Push(operands);

                try
                {
                    foreach ( var nestedFactory in nestedFactories )
                    {
                        nestedFactory(this);
                    }
                }
                finally
                {
                    _nestedMatchers.Pop();
                }

                PushMatcher(compositeFactory(operands.ToArray()));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PushMultiplier(ILogexMultiplier multiplier)
            {
                if ( _template != null )
                {
                    throw new InvalidOperationException("Logex builder is expecting a matcher, but got multiplier.");
                }

                _template = new LogexImpl.SingleSegmentTemplate(multiplier);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void PushMatcher(ILogexNodeMatcher matcher)
            {
                if ( _nestedMatchers.Count > 0 )
                {
                    _nestedMatchers.Peek().Add(matcher);
                }
                else if ( _template != null )
                {
                    _segments.AddRange(_template.CreateSegments(matcher));
                }
                else
                {
                    throw new InvalidOperationException("Logex builder is expecting a multiplier, but got matcher.");
                }

                _template = null;
            }
        }
    }
}

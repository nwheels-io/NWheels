using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Testing.Logging.Core;
using NWheels.Logging;
using NWheels.Logging.Core;
using System.Linq.Expressions;
using NUnit.Framework;
using NWheels.Extensions;

namespace NWheels.Testing.Logging.Impl
{
    internal static class LogexImpl
    {
        public static string Describe(this LogNode node)
        {
            return string.Format(
                "{0}|{1}({2})", 
                node.Level, 
                node.MessageId, 
                string.Join(",", node.NameValuePairs.Where(p => !p.IsBaseValue()).Select(p => p.FormatLogString())));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ILogexValueMatcher CreateValueMatcher(Expression expectedValueExpression)
        {
            var constantExpression = (expectedValueExpression as ConstantExpression);
            var callExpression = (expectedValueExpression as MethodCallExpression);

            if ( constantExpression != null )
            {
                return new ExactValueMatcher(constantExpression.Value);
            }
            else if ( callExpression != null && callExpression.Method.DeclaringType == typeof(Logex) )
            {
                if ( callExpression.Method.Name == "Any" )
                {
                    return new AnyValueMatcher();
                }
                else
                {
                    throw new AssertionException(string.Format("Value matcher not recognized: '{0}'", callExpression.Method.Name));
                }
            }
            else
            {
                var valueFunc = (Func<object>)Expression.Lambda(Expression.Convert(expectedValueExpression, typeof(object))).Compile();
                var value = valueFunc();
                return new ExactValueMatcher(value);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AnyNodeMatcher : ILogexNodeMatcher
        {
            public bool Match(LogNode node)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return "Any";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ByLevelNodeMatcher : ILogexNodeMatcher
        {
            private readonly LogLevel[] _levels;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public ByLevelNodeMatcher(LogLevel[] levels)
            {
                _levels = levels;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return _levels.Contains(node.Level);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Format("of level '{0}'", string.Join(",", _levels));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ByLevelOrHigherNodeMatcher : ILogexNodeMatcher
        {
            private readonly LogLevel _level;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ByLevelOrHigherNodeMatcher(LogLevel level)
            {
                _level = level;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return (node.Level >= _level);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Format("of level '{0}' or higher", _level);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ByLevelOrLowerNodeMatcher : ILogexNodeMatcher
        {
            private readonly LogLevel _level;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ByLevelOrLowerNodeMatcher(LogLevel level)
            {
                _level = level;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return (node.Level <= _level);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Format("of level '{0}' or lower", _level);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ByLoggerNodeMatcher : ILogexNodeMatcher
        {
            private readonly Type _loggerInterface;
            private readonly string _messageIdClassifier;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ByLoggerNodeMatcher(Type loggerInterface)
            {
                _loggerInterface = loggerInterface;
                _messageIdClassifier = ApplicationEventLoggerConvention.GetMessageIdClassifier(loggerInterface);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return (
                    node.MessageId.StartsWith(_messageIdClassifier) && 
                    node.MessageId.IndexOf(".", _messageIdClassifier.Length + 1, StringComparison.Ordinal) < 0);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Format("from logger {0}", _loggerInterface.FullName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ByMessageNodeMatcher : ILogexNodeMatcher
        {
            private readonly string _messageId;
            private readonly string[] _argumentNames;
            private readonly ILogexValueMatcher[] _argumentValueMatchers;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ByMessageNodeMatcher(LambdaExpression messageSelector)
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                var parameterDescriptions = new string[_argumentNames.Length];

                for ( int i = 0 ; i < _argumentNames.Length ; i++ )
                {
                    parameterDescriptions[i] = string.Format("{0}{1}", _argumentNames[i], _argumentValueMatchers[i].Describe());
                }

                return string.Format("message {0}({1})", _messageId, string.Join(",", parameterDescriptions));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EndOfLogMatcher : ILogexNodeMatcher
        {
            public bool Match(LogNode node)
            {
                return false;
            }
            public bool MatchEndOfInput()
            {
                return true;
            }
            public string Describe()
            {
                return "end of log";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OperatorAndNodeMatcher : ILogexNodeMatcher
        {
            private readonly ILogexNodeMatcher[] _operands;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OperatorAndNodeMatcher(params ILogexNodeMatcher[] operands)
            {
                _operands = operands;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return _operands.All(operand => operand.Match(node));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return _operands.All(operand => operand.MatchEndOfInput());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Join(" AND ", _operands.Select(m => "(" + m.Describe() + ")"));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OperatorOrNodeMatcher : ILogexNodeMatcher
        {
            private readonly ILogexNodeMatcher[] _operands;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OperatorOrNodeMatcher(params ILogexNodeMatcher[] operands)
            {
                _operands = operands;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return _operands.Any(operand => operand.Match(node));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return _operands.Any(operand => operand.MatchEndOfInput());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Join(" OR ", _operands.Select(m => "(" + m.Describe() + ")"));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OperatorNotNodeMatcher : ILogexNodeMatcher
        {
            private readonly ILogexNodeMatcher _operand;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OperatorNotNodeMatcher(ILogexNodeMatcher operand)
            {
                _operand = operand;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Match(LogNode node)
            {
                return !_operand.Match(node);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MatchEndOfInput()
            {
                return !_operand.MatchEndOfInput();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return "NOT(" + _operand.Describe() + ")";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RangeOfTimesMultiplier : ILogexMultiplier
        {
            private readonly int _minTimes;
            private readonly int _maxTimes;
            private readonly string _description;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public RangeOfTimesMultiplier(int minTimes, int maxTimes, string description = null)
            {
                _maxTimes = maxTimes;
                _minTimes = minTimes;
                _description = description;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsSatisfied(int matchCount)
            {
                return (matchCount >= _minTimes);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsAtMax(int matchCount)
            {
                return (matchCount >= _maxTimes);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string Describe()
            {
                return (_description ?? string.Format("{0}..{1} times", _minTimes, _maxTimes));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExactValueMatcher : ILogexValueMatcher
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
                if (object.ReferenceEquals(actualValue, _expectedValue))
                {
                    return true;
                }

                if (actualValue != null && _expectedValue != null)
                {
                    return actualValue.Equals(_expectedValue);
                }

                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return string.Format("={0}", _expectedValue != null ? _expectedValue.ToString() : "null");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AnyValueMatcher : ILogexValueMatcher
        {
            public bool Match(object actualValue)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Describe()
            {
                return " is anything";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SingleSegmentTemplate : ILogexTemplate
        {
            private readonly ILogexMultiplier _multiplier;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SingleSegmentTemplate(ILogexMultiplier multiplier)
            {
                _multiplier = multiplier;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LogexSegment[] CreateSegments(ILogexNodeMatcher matcher)
            {
                return new[] {
                    new LogexSegment(matcher, _multiplier)
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NoneToEndTemplate : ILogexTemplate
        {
            public LogexSegment[] CreateSegments(ILogexNodeMatcher matcher)
            {
                return new[] {
                    new LogexSegment(new OperatorNotNodeMatcher(matcher), new RangeOfTimesMultiplier(0, Int32.MaxValue, "to end of log")),
                    new LogexSegment(new EndOfLogMatcher(), new RangeOfTimesMultiplier(1, 1, "once"))
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AllToEndTemplate : ILogexTemplate
        {
            public LogexSegment[] CreateSegments(ILogexNodeMatcher matcher)
            {
                return new[] {
                    new LogexSegment(matcher, new RangeOfTimesMultiplier(0, Int32.MaxValue, "to end of log")),
                    new LogexSegment(new EndOfLogMatcher(), new RangeOfTimesMultiplier(1, 1, "once"))
                };
            }
        }
    }
}

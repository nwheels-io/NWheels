using System;
using NWheels.Logging;
using NWheels.Testing.Logging.Core;

namespace NWheels.Testing.Logging.Impl
{
    internal class LogexSegment
    {
        public LogexSegment(ILogexNodeMatcher matcher, ILogexMultiplier multiplier)
        {
            this.Matcher = matcher;
            this.Multiplier = multiplier;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Match(LogNode[] input, int inputIndex, out string mismatchDescription)
        {
            mismatchDescription = null;
            var matchCount = 0;

            while ( true )
            {
                var isEndOfInput = (inputIndex >= input.Length);

                if ( Multiplier.IsSatisfied(matchCount) )
                {
                    if ( NextSegment != null )
                    {
                        if ( NextSegment.Match(input, inputIndex, out mismatchDescription) )
                        {
                            return true;
                        }
                    }
                    else
                    {
                        mismatchDescription = null;
                        return true;
                    }
                }

                if ( Multiplier.IsAtMax(matchCount) )
                {
                    return false;
                }

                if ( isEndOfInput )
                {
                    if ( Matcher.MatchEndOfInput() )
                    {
                        mismatchDescription = null;
                        return true;
                    }
                    else
                    {
                        DescribeMismatchIfNotDescribed(ref mismatchDescription, "end of log: expected '{0}'", Matcher.Describe());
                        //if ( string.IsNullOrEmpty(mismatchDescription) )
                        //{
                        //    mismatchDescription = string.Format("end of log: expected '{0}'", Matcher.Describe());
                        //}
                        return false;
                    }
                }

                if ( !Matcher.Match(input[inputIndex]) )
                {
                    DescribeMismatchIfNotDescribed(
                        ref mismatchDescription,
                        "[{0}] expected: '{1}' but was: '{2}'",
                        inputIndex, Matcher.Describe(), input[inputIndex].Describe());
                    //mismatchDescription = string.Format(
                    //    "[{0}] expected: '{1}' but was: '{2}'",
                    //    inputIndex, Matcher.Describe(), input[inputIndex].Describe());
                    return false;
                }

                matchCount++;
                inputIndex++;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void DescribeMismatchIfNotDescribed(ref string mismatchDescription, string format, params object[] args)
        {
            if ( String.IsNullOrEmpty(mismatchDescription) )
            {
                mismatchDescription = String.Format(format, args);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string Describe()
        {
            return String.Format("{{{0}}}{1}", Multiplier.Describe(), Matcher.Describe());
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogexNodeMatcher Matcher { get; private set; }
        public ILogexMultiplier Multiplier { get; private set; }
        public LogexSegment NextSegment { get; set; }
    }
}
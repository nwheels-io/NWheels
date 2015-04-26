using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Extensions;
using NWheels.Logging;

namespace NWheels.Testing
{
    public static class LogAssert
    {
        public static void Empty(IEnumerable<LogNode> log)
        {
            var logArray = log.ToArray();

            if ( logArray.Length > 0 )
            {
                Assert.Fail("Expected no log messages, but was: {0}", logArray.Length);
            }
        }

        public static Func<LogNode, bool> MatchId<TLogger>(Expression<Action<TLogger>> messageSelector)
        {
            messageSelector.GetMethodInfo();
            return (n => true);
        }


//        public static bool MatchId

        //public static void AssertNoErrors(this IEnumerable<LogNode> log)
        //{

        //}

        //public static void AssertNoErrorsNoWarnings(this IEnumerable<LogNode> log)
        //{

        //}

        //public static void AssertNone(this IEnumerable<LogNode> log, Func<LogNode, bool> predicate)
        //{

        //}

        //public static void AssertNone<TLogger>(Action<TLogger> message)
        //{

        //}

        //public LogAssert FindNone<TLogger>(Action<TLogger> message)
        //{

        //}

        //public LogAssert MoveToFirst<TLogger>(Action<TLogger> message)
        //{

        //}
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using NWheels.Logging;

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class TestFixtureBase
    {
        [SetUp]
        public void MyBaseSetUp()
        {
            Assertions = new AssertsCollector();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AssertsCollector Assertions { get; private set; }
        abstract protected ITestFixtureBaseLogger Logger { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected class AssertsCollector
        {
            private readonly StringBuilder _checksResults;
            private bool _isAllSucceeded;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public string FullMessage { get { return _checksResults.ToString(); } }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public AssertsCollector()
            {
                _checksResults = new StringBuilder();
                _isAllSucceeded = true;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Include(Expression<Action> actionExpression)
            {
                var assertText = actionExpression.ToString();
                var action = actionExpression.Compile();
                var argumentNames = ((MethodCallExpression)actionExpression.Body).Method.GetParameters().Select(p => p.Name).ToArray();
                var argumentValues = (
                    from arg in ((MethodCallExpression)actionExpression.Body).Arguments
                    let argAsObj = Expression.Convert(arg, typeof(object))
                    select Expression.Lambda<Func<object>>(argAsObj, null)
                    .Compile()()).ToArray();

                try
                {
                    action();

                    StringBuilder sb = new StringBuilder();
                    for ( int i = 0 ; i < argumentNames.Length ; i++ )
                    {
                        sb.Append(string.Format(" {0} : {1};", argumentNames[i], argumentValues[i]));
                    }
                    _checksResults.AppendLine(string.Format("Success: {0}", sb));
                    return true;
                }
                catch ( AssertionException ex)
                {
                    _checksResults.Append(string.Format("Failed: {0}", ex.Message));
                    _isAllSucceeded = false;
                    return false;
                }
            }

            public void AssertAllSucceeded(ITestFixtureBaseLogger logger)
            {
                try
                {
                    Assert.IsTrue(_isAllSucceeded);
                    logger.TestPassed("\r\n" + FullMessage);
                }
                catch ( AssertionException )
                {
                    logger.TestFailed("\r\n" + FullMessage);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestFixtureBaseLogger : IApplicationEventLogger
        {
            [LogInfo]
            void TestPassed([Detail(MaxStringLength = 16384, IncludeInSingleLineText = true)] string assertions);
            [LogError]
            void TestFailed([Detail(MaxStringLength = 16384, IncludeInSingleLineText = true)] string assertions);
        }
    }
}

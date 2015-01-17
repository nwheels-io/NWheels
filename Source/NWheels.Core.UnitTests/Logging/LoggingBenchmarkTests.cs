using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NWheels.Core.UnitTests.Logging
{
    [TestFixture]
    public class LoggingBenchmarkTests
    {
        private List<KeyValuePair<string, long>> _millisecondsByTestCaseName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _millisecondsByTestCaseName = new List<KeyValuePair<string, long>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            var output = new StringBuilder();

            foreach ( var testCaseGroup in _millisecondsByTestCaseName.GroupBy(kvp => kvp.Key, kvp => kvp.Value) )
            {
                var minDuration = testCaseGroup.Min();
                var maxDuration = testCaseGroup.Max();
                var averageDuration = testCaseGroup.Average();

                output.AppendFormat("{0} : min={1} ms , max={2} ms , avg={3} ms", testCaseGroup.Key, minDuration, maxDuration, averageDuration);
                output.AppendLine();
            }

            File.WriteAllText(@"D:\LoggingBenchmarkTests-results.txt", output.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TearDown]
        public void TearDown()
        {
            //s_ThreadStaticValue = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TestCaseData CreateScopeStrategyTestCase<TStrategy>(Func<IDisposable> environmentFactory)
            where TStrategy : ScopeStrategyUnderTest, new()
        {
            return new TestCaseData(new Func<ScopeStrategyUnderTest>(() => new TStrategy()), environmentFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TestCaseData[] s_ScopeStrategyTestCases = {
            CreateScopeStrategyTestCase<InstanceFieldScopeStrategy>(NullEnvironmentFactory).SetName("Instance Field / Null Environment"),
            CreateScopeStrategyTestCase<ThreadStaticScopeStrategy>(NullEnvironmentFactory).SetName("Thread Static / Null Environment"),
            CreateScopeStrategyTestCase<UniversalScopeStrategy>(NullEnvironmentFactory).SetName("Universal / Null Environment"),
            CreateScopeStrategyTestCase<UniversalScopeStrategy>(AspNetEnvironmentFactory).SetName("Universal / ASP.NET Environment"),
            CreateScopeStrategyTestCase<UniversalScopeStrategy>(WcfEnvironmentFactory).SetName("Universal / WCF Environment"),
        };
        private static TestCaseData[] s_ScopeStrategyTestCasesRepeated_x10 = 
            s_ScopeStrategyTestCases
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases)
            .Concat(s_ScopeStrategyTestCases).ToArray();
        private static TestCaseData[] s_ScopeStrategyTestCasesRepeated_x100 =
            s_ScopeStrategyTestCasesRepeated_x10
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10)
            .Concat(s_ScopeStrategyTestCasesRepeated_x10).ToArray();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCaseSource("s_ScopeStrategyTestCasesRepeated_x100")]
        public void ThreadStatic_SingleThread(Func<ScopeStrategyUnderTest> scopeFactory, Func<IDisposable> environmentFactory)
        {
            const int cyclesPerUnitOfWork = 10000;

            using ( environmentFactory() )
            {
                var watch = Stopwatch.StartNew();

                RunUnitOfWork(cyclesPerUnitOfWork, scopeFactory);

                var elapsedMilliseconds = watch.ElapsedMilliseconds;

                _millisecondsByTestCaseName.Add(new KeyValuePair<string, long>(TestContext.CurrentContext.Test.FullName, elapsedMilliseconds));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCaseSource("s_ScopeStrategyTestCasesRepeated_x100")]
        public void ThreadStatic_MultipleThreads(Func<ScopeStrategyUnderTest> scopeFactory, Func<IDisposable> environmentFactory)
        {
            const int threadCount = 25;
            const int cyclesPerUnitOfWorkPerThread = 10000;

            var testName = TestContext.CurrentContext.Test.FullName;
            var syncRoot = new object();
            var startEvent = new ManualResetEvent(initialState: false);
            var threadFunc = new ThreadStart(() => {
                using ( environmentFactory() )
                {
                    startEvent.WaitOne();

                    var watch = Stopwatch.StartNew();

                    RunUnitOfWork(cyclesPerUnitOfWorkPerThread, scopeFactory);

                    var elapsedMilliseconds = watch.ElapsedMilliseconds;

                    lock ( syncRoot )
                    {
                        _millisecondsByTestCaseName.Add(new KeyValuePair<string, long>(testName, elapsedMilliseconds));
                    }
                }
            });

            var threads = new Thread[threadCount];

            for ( int i = 0 ; i < threads.Length ; i++ )
            {
                threads[i] = new Thread(threadFunc);
                threads[i].Start();
            }

            startEvent.Set();

            for ( int i = 0 ; i < threads.Length ; i++ )
            {
                threads[i].Join();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int RunUnitOfWork(int cycles, Func<ScopeStrategyUnderTest> scopeFactory)
        {
            var generator = RNGCryptoServiceProvider.Create();
            var hashOfHash = 0;

            for ( int i = 0 ; i < cycles ; i++ )
            {
                using ( var scope = scopeFactory() )
                {
                    scope.CurrentValue = new object();

                    byte[] key = new byte[1024];
                    generator.GetBytes(key);

                    var hash = 0;

                    for ( int j = 0 ; j < key.Length ; j++ )
                    {
                        hash ^= key[j];
                    }

                    hashOfHash ^= hash;
                }
            }

            return hashOfHash;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IDisposable NullEnvironmentFactory()
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IDisposable AspNetEnvironmentFactory()
        {
            System.Web.HttpContext.Current = new System.Web.HttpContext(
                new System.Web.HttpRequest(null, "http://tempuri.org", null),
                new System.Web.HttpResponse(null));

            return new HttpContextCleaner();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IDisposable WcfEnvironmentFactory()
        {
            var channelFactory = new System.ServiceModel.ChannelFactory<IDummyServiceContract>(new BasicHttpBinding(), "http://dummy.com");
            return new System.ServiceModel.OperationContextScope((IContextChannel)channelFactory.CreateChannel());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ScopeStrategyUnderTest : IDisposable
        {
            public virtual void Dispose()
            {
                this.CurrentValue = null;
            }
            public abstract object CurrentValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InstanceFieldScopeStrategy : ScopeStrategyUnderTest
        {
            public override object CurrentValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadStaticScopeStrategy : ScopeStrategyUnderTest
        {
            public override object CurrentValue
            {
                get
                {
                    return s_Current;
                }
                set
                {
                    s_Current = value;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [ThreadStatic]
            private static object s_Current;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class UniversalScopeStrategy : ScopeStrategyUnderTest
        {
            public override object CurrentValue
            {
                get
                {
                    System.Web.HttpContext httpContext;
                    System.ServiceModel.OperationContext operationContext;

                    if ( (httpContext = System.Web.HttpContext.Current) != null )
                    {
                        return httpContext.Items[s_HttpContextKey];
                    }
                    else if ( (operationContext = System.ServiceModel.OperationContext.Current) != null )
                    {
                        var extension = operationContext.Extensions.Find<OperationContextExtension>();
                        return (extension != null ? extension.CurrentValue : null);
                    }
                    else
                    {
                        return s_ThreadStaticCurrentValue;
                    }
                }
                set
                {
                    System.Web.HttpContext httpContext;
                    System.ServiceModel.OperationContext operationContext;

                    if ( (httpContext = System.Web.HttpContext.Current) != null )
                    {
                        httpContext.Items[s_HttpContextKey] = value;
                    }
                    else if ( (operationContext = System.ServiceModel.OperationContext.Current) != null )
                    {
                        var extension = operationContext.Extensions.Find<OperationContextExtension>();

                        if ( extension == null )
                        {
                            extension = new OperationContextExtension();
                            operationContext.Extensions.Add(extension);
                        }
                        
                        extension.CurrentValue = value;
                    }
                    else
                    {
                        s_ThreadStaticCurrentValue = value;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [ThreadStatic]
            private static object s_ThreadStaticCurrentValue;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly object s_HttpContextKey = new object();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private class OperationContextExtension : System.ServiceModel.IExtension<System.ServiceModel.OperationContext>
            {
                void System.ServiceModel.IExtension<System.ServiceModel.OperationContext>.Attach(System.ServiceModel.OperationContext owner)
                {
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                void System.ServiceModel.IExtension<System.ServiceModel.OperationContext>.Detach(System.ServiceModel.OperationContext owner)
                {
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public object CurrentValue { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class HttpContextCleaner : IDisposable
        {
            public void Dispose()
            {
                System.Web.HttpContext.Current = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [System.ServiceModel.ServiceContract]
        public interface IDummyServiceContract
        {
            [System.ServiceModel.OperationContract]
            System.ServiceModel.Channels.Message ProcessRequest(System.ServiceModel.Channels.Message request);
        }
    }
}

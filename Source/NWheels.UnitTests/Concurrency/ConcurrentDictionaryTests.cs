using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Testing;

namespace NWheels.UnitTests.Concurrency
{
    [TestFixture, Category(TestCategory.Load)]
    public class ConcurrentDictionaryTests
    {

        [Test]
        public void TestConcurrentDictionary()
        {
            ExecuteTest(new ConcurrentDictionary<int, string>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestHashtable()
        {
            ExecuteTest(new Hashtable());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteTest(IDictionary dataStructure)
        {
            var runEvent = new ManualResetEvent(false);

            for ( int i = 0; i < 1000; i++ )
            {
                dataStructure[i] = i.ToString();
            }

            var readers = new List<Thread>();
            var writers = new List<Thread>();

            for ( int readerIndex = 0 ; readerIndex < 10 ; readerIndex++ )
            {
                var reader = new Thread(() => {
                    runEvent.WaitOne();
                    for ( int iteration = 0; iteration < 10000 ; iteration++ )
                    {
                        var value = dataStructure[iteration % dataStructure.Count];
                    }
                });

                readers.Add(reader);
            }

            var writerSyncRoot = new object();

            for ( int writerIndex = 0; writerIndex < 5 ; writerIndex++ )
            {
                var writer = new Thread(() => {
                    runEvent.WaitOne();
                    int iteration = 0;
                    while ( runEvent.WaitOne(0) )
                    {
                        lock ( writerSyncRoot )
                        {
                            dataStructure[900 + iteration++] = "CHANGED!";
                        }
                        Thread.Sleep(100);
                    }
                });

                writers.Add(writer);
            }

            foreach ( var reader in readers )
            {
                reader.Start();
            }

            foreach ( var writer in writers )
            {
                writer.Start();
            }

            var clock = Stopwatch.StartNew();

            runEvent.Set();

            foreach ( var reader in readers )
            {
                reader.Join();
            }

            var time = clock.Elapsed;

            runEvent.Reset();

            foreach ( var writer in writers )
            {
                writer.Join();
            }

            Console.WriteLine("{0} : {1}", TestContext.CurrentContext.Test.Name, time);
        }
    }
}

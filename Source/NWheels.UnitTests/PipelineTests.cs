using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Core;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests
{
    [TestFixture]
    public class PipelineTests : UnitTestBase
    {
        [Test]
        public void SingleSinkAsService_InvokeVoidParameterlessMethod()
        {
            //-- arrange
            
            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[] { new LoggingTestPipeSink("A", log) });

            //-- act

            var pipelineAsService = pipeline.AsService();
            pipelineAsService.Zero();

            //-- assert

            log.ShouldBe(new[] { "A>Zero()" });
            pipelineAsService.ShouldBeSameAs(pipeline[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MultipleSinksAsService_InvokeVoidParameterlessMethod()
        {
            //-- arrange

            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[] {
                new LoggingTestPipeSink("A", log),
                new LoggingTestPipeSink("B", log),
                new LoggingTestPipeSink("C", log)
            }, Resolve<PipelineObjectFactory>());

            //-- act

            var pipelineAsService = pipeline.AsService();
            pipelineAsService.Zero();

            //-- assert

            log.ShouldBe(new[] {
                "A>Zero()",
                "B>Zero()",
                "C>Zero()",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MultipleSinksAsService_InvokeVoidMethodWithParameters()
        {
            //-- arrange

            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[] {
                new LoggingTestPipeSink("A", log),
                new LoggingTestPipeSink("B", log),
                new LoggingTestPipeSink("C", log)
            }, Resolve<PipelineObjectFactory>());

            //-- act

            var pipelineAsService = pipeline.AsService();
            pipelineAsService.Two(n: 123, s: "ABC");

            //-- assert

            log.ShouldBe(new[] {
                "A>Two(123,ABC)",
                "B>Two(123,ABC)",
                "C>Two(123,ABC)",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void MultipleSinksAsService_InvokeVoidMethodWithRefParameters()
        {
            //-- arrange

            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[] {
                new LoggingTestPipeSink("A", log),
                new LoggingTestPipeSink("B", log),
                new LoggingTestPipeSink("C", log)
            }, Resolve<PipelineObjectFactory>());

            //-- act

            var pipelineAsService = pipeline.AsService();
            var stringValue = "ABC";
            pipelineAsService.Three(123, ref stringValue);

            //-- assert

           log.ShouldBe(new[] {
                "A>Three(123,ABC)",
                "B>Three(123,A:Three:s)",
                "C>Three(123,B:Three:s)",
            });

            stringValue.ShouldBe("C:Three:s");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NoSinksAsService_InvokeMethod_DoNothing()
        {
            //-- arrange

            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[0], Resolve<PipelineObjectFactory>());

            //-- act

            var pipelineAsService = pipeline.AsService();
            pipelineAsService.Zero();

            //-- assert

            log.ShouldBeEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void InvokeNonVoidMethod_Throw()
        {
            //-- arrange

            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[] {
                new LoggingTestPipeSink("A", log),
                new LoggingTestPipeSink("B", log),
                new LoggingTestPipeSink("C", log)
            }, Resolve<PipelineObjectFactory>());

            //-- act

            var pipelineAsService = pipeline.AsService();
            pipelineAsService.Four(123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void InvokeVoidMethodWithOutParameter_Throw()
        {
            //-- arrange

            var log = new List<string>();
            var pipeline = new Pipeline<ITestPipeSink>(new ITestPipeSink[] {
                new LoggingTestPipeSink("A", log),
                new LoggingTestPipeSink("B", log),
                new LoggingTestPipeSink("C", log)
            }, Resolve<PipelineObjectFactory>());

            //-- act

            var pipelineAsService = pipeline.AsService();

            string stringValue;
            pipelineAsService.Five(123, out stringValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestPipeSink
        {
            void Zero();
            void One(int n);
            void Two(int n, string s);
            void Three(int n, ref string s);
            string Four(int n);
            void Five(int n, out string s);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoggingTestPipeSink : ITestPipeSink
        {
            private readonly string _name;
            private readonly List<string> _log;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoggingTestPipeSink(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITestPipeSink

            public void Zero()
            {
                _log.Add(string.Format("{0}>Zero()", _name));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void One(int n)
            {
                _log.Add(string.Format("{0}>One({1})", _name, n));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Two(int n, string s)
            {
                _log.Add(string.Format("{0}>Two({1},{2})", _name, n, s));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Three(int n, ref string s)
            {
                _log.Add(string.Format("{0}>Three({1},{2})", _name, n, s));
                s = _name + ":Three:s";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Four(int n)
            {
                _log.Add(string.Format("{0}>Four({1})", _name, n));
                return _name + ":Four:s";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Five(int n, out string s)
            {
                _log.Add(string.Format("{0}>Five({1},{2})", _name, n, "?"));
                s = _name + ":Five:s";
            }

            #endregion
        }
    }
}

using FluentAssertions;
using Newtonsoft.Json;
using NWheels.Frameworks.Uidl.Testability;
using NWheels.Testability.Microservices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Xunit;

namespace NWheels.Samples.FirstHappyPath.HelloService.Tests.System
{
    [Trait("Purpose", "SystemTest")]
    /*public*/ internal class HelloWorldAppTests : UidlE2eTestBase<HelloWorldApp>
    {
        private readonly ClassFixture _fixture;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HelloWorldAppTests(ClassFixture fixture)
        {
            _fixture = fixture;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanNavigateStartPage()
        {
            //-- when

            When(() => Driver.NavigateToStartPage());

            //-- then

            Then(() => Driver.CurrentPage<HelloWorldApp.HomePage>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanExecuteHelloTransaction()
        {
            //-- given

            Given(() => Driver.NavigateToStartPage());

            var tx = Driver.CurrentPage<HelloWorldApp.HomePage>().Transaction;

            Given(() => tx.Element(x => x.Name).IsEnabled());

            //-- when

            When(() => tx.Element(x => x.Name).TypeText("John"));
            When(() => tx.SubmitCommand.Click());

            //-- then

            Then(() => tx.Element(x => x.Message).IsVisible());
            Then(() => tx.Element(x => x.Message).HasText("Hello world, from John!"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void InitiallySubmitButtonIsDisabled()
        {
            //-- when

            When(() => Driver.NavigateToStartPage());

            //-- then

            Then(() => Driver.CurrentPage<HelloWorldApp.HomePage>().Transaction.SubmitCommand.IsDisabled());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void EmptyingNameFieldRendersInputFormInvalid()
        {
            //-- given

            Given(() => Driver.NavigateToStartPage());

            var tx = Driver.CurrentPage<HelloWorldApp.HomePage>().Transaction;

            Given(() => tx.Element(x => x.Name).TypeText("John"));

            //-- when

            When(() => tx.Element(x => x.Name).ClearValue());

            //-- then

            Then(() => tx.InputForm.IsInvalid());
            Then(() => tx.SubmitCommand.IsDisabled());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TypingIntoNameFieldMakesInputFormValid()
        {
            //-- given

            Given(() => Driver.NavigateToStartPage());

            var tx = Driver.CurrentPage<HelloWorldApp.HomePage>().Transaction;

            Given(() => tx.Element(x => x.Name).TypeText("John"));
            Given(() => tx.Element(x => x.Name).ClearValue());

            //-- when

            When(() => tx.Element(x => x.Name).TypeText("Smith"));

            //-- then

            Then(() => tx.InputForm.IsValid());
            Then(() => tx.SubmitCommand.IsEnabled());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ClassFixture : IDisposable
        {
            private readonly MicroserviceController _controller;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClassFixture()
            {
                var configurationName = MicroserviceControllerBuilder.DefaultProjectConfigurationName;

                _controller = new MicroserviceControllerBuilder()
                    .RunMicroserviceAssembly($@"..\..\NWheels.Samples.FirstHappyPath.HelloService\bin\{configurationName}\netcoreapp1.1\hello.dll")
                    .Build();

                _controller.Start();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _controller.StopOrThrow(10000);
                _controller.AssertNoErrors();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MicroserviceController Controller => _controller;
        }
    }
}

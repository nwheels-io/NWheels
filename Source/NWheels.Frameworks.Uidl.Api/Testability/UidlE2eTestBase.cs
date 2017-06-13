using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Frameworks.Uidl.Testability
{
    public abstract class UidlE2eTestBase<TApp>
        where TApp : class, IAbstractUIApp
    {
        public void Given(Action code)
        {
        }
        public void Given(Func<bool> code)
        {
        }
        public void Given(Func<Task> code)
        {
        }
        public void Given(Func<Task<bool>> code)
        {
        }

        public void When(Action code)
        {
        }
        public void When(Func<Task> code)
        {
        }

        public void Then(Action code)
        {
        }
        public void Then(Func<Task> code)
        {
        }
        public void Then(Func<Task<bool>> code)
        {
        }

        protected TApp AppUnderTest { get; }
        protected WebUIAppDriver<TApp> Driver { get; }
    }
}

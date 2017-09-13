using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Api.Execution
{
    public interface IExecutionPathActivity : IDisposable
    {
        void Fail(Exception error);
        void Fail(string reason);
    }
}

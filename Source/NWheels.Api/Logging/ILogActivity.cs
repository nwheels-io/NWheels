using System;

namespace NWheels.Logging
{
    public interface ILogActivity : IDisposable
    {
        void Warn(Exception error);

        void Fail(Exception error);
    }
}

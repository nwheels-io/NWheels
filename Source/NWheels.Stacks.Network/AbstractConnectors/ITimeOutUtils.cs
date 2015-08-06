using System;

namespace NWheels.Stacks.Network
{
    public interface ITimeOutUtils : IDisposable
    {
        void Run(OnTimeOutInvokerDlgt managerTimeOutInvoker);

        void Run(OnTimeOutInvokerDlgt managerTimeOutInvoker, TimeOutResolution timeResolution);
        TimeOutHandle AddTimeOutEvent(UInt32 timeOutInSeconds, OnTimeOutDlgt timeOutdlgt, object timeOutdlgtParam);

        TimeOutHandle AddMsTimeOutEvent(UInt32 timeOutInMilliSeconds, OnTimeOutDlgt timeOutdlgt, object timeOutdlgtParam);

        void CancelTimeOutEvent(TimeOutHandle handle);
    }

    public enum TimeOutResolution
    {
        //MilliSeconds,
        TensMilliSeconds,
        Seconds             // Default
    }
}
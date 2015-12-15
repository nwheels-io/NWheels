using System;

namespace NWheels.Concurrency
{
    public interface IResourceConsumerScopeHandle : IDisposable
    {
        int NestingLevel { get; }
        bool IsInnermost { get; }
        bool IsOutermost { get; }
        bool ForceNewResource { get; }
        IResourceConsumerScopeHandle Innermost { get; }
        IResourceConsumerScopeHandle Outermost { get; }
    }
}

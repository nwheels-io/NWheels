using System.Collections.Generic;
using System.Reflection;

namespace NWheels.DataObjects
{
    public interface ITypeLogicMetadata : IMetadataElement
    {
        IReadOnlyList<MethodInfo> OnNewTriggerMethods { get; }
        IReadOnlyList<MethodInfo> OnValidateInvariantsTriggerMethods { get; }
        IReadOnlyList<MethodInfo> OnBeforeSaveTriggerMethods { get; }
        IReadOnlyList<MethodInfo> OnAfterSaveTriggerMethods { get; }
        IReadOnlyList<MethodInfo> OnBeforeDeleteTriggerMethods { get; }
        IReadOnlyList<MethodInfo> OnAfterDeleteTriggerMethods { get; }
    }
}

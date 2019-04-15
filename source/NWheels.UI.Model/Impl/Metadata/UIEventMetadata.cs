using System.Collections.Generic;

namespace NWheels.UI.Model.Impl.Metadata
{
    public class UIEventMetadata
    {
        public string Name { get; set; }
        public List<UIFunctionMetadata> Listeners { get; set; } = new List<UIFunctionMetadata>();
    }
    
    public class UIFunctionMetadata
    {
        public List<UIBehavior> Steps { get; set; } = new List<UIBehavior>();
    }

    public abstract class UIBehavior
    {
    }

    public class UIConstantBehavior : UIBehavior
    {
        public object Value { get; set; }
    }

    public class UIStateReadBehavior : UIBehavior
    {
        public string StatePropertyName { get; set; }
    }

    public class UIStateMutationBehavior : UIBehavior
    {
        public string StatePropertyName { get; set; }
        public UIBehavior NewValue { get; set; }
    }

    public class UIFetchBehavior : UIBehavior
    {
        public string BackendApiName { get; set; }
        public string OperationName { get; set; }
        public List<UIBehavior> Arguments { get; set; } = new List<UIBehavior>();
    }
}
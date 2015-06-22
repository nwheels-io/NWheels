namespace NWheels.UI.OldToolbox
{
    public interface IAccessParentState<TParentState>
    {
        TParentState ParentState { get; set; }
    }
}

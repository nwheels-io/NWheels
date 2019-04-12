namespace NWheels.UI.Model
{
    public interface ILayoutComponent
    {
    }
    
    public class LayoutComponent<TProps, TState> : UIComponent<TProps, TState>, ILayoutComponent
    {
    }
}

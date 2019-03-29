using System;

namespace NWheels.UI.Model
{
    public abstract class PropsOf<TComponent>
        where TComponent : UIComponent
    {
        public static implicit operator TComponent(PropsOf<TComponent> props) 
            => default(TComponent);
    }


    public abstract class UIComponent
    {
        public static implicit operator UIComponent(string text)
        {
            return new TextContent(text);
        }
    }
        
        
    public abstract class UIComponent<TProps, TState> : UIComponent
    {
        public TProps Props => default(TProps);
        public TState State => default(TState);

        protected void SetState(TState nextState)
        {
        }
        
        protected virtual void Controller()
        {
        }

        protected TComponent FromProps<TComponent>(PropsOf<TComponent> props)
            where TComponent : UIComponent
            => default;
    }
}

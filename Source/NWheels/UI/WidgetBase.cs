using System.Linq;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public abstract class WidgetBase<TWidget, TData, TState> : WidgetUidlNode, UidlBuilder.IBuildableUidlNode
        where TWidget : WidgetBase<TWidget, TData, TState>
        where TData : class
        where TState : class
    {
        protected WidgetBase(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            base.ModelDataType = builder.RegisterMetaType(typeof(TData));
            base.ModelStateType = builder.RegisterMetaType(typeof(TState));
                
            builder.InstantiateDeclaredMemberNodes(this);
            builder.BuildNodes(this.GetNestedWidgets().Cast<AbstractUidlNode>().ToArray());

            DescribePresenter(new PresenterBuilder<TWidget, TData, TState>(builder, this));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TWidget, TData, TState> presenter);
    }
}
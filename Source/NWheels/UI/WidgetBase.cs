using System.Collections.Generic;
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

            var childNodesToBuild = new HashSet<AbstractUidlNode>();
            childNodesToBuild.UnionWith(builder.GetDeclaredMemberNodes(this));
            childNodesToBuild.UnionWith(this.GetNestedWidgets());
            
            builder.BuildNodes(childNodesToBuild.ToArray());

            OnBuild(builder);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            DescribePresenter(new PresenterBuilder<TWidget, TData, TState>(builder, this));
            builder.DescribeNodePresenters(this.GetNestedWidgets().Cast<AbstractUidlNode>().ToArray());
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TWidget, TData, TState> presenter);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnBuild(UidlBuilder builder)
        {
        }
    }
}

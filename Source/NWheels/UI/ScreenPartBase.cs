using System.Linq;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public abstract class ScreenPartBase<TScreenPart, TInput, TData, TState> : UidlScreenPart, UidlBuilder.IBuildableUidlNode, IScreenPartWithInput<TInput>
        where TScreenPart : ScreenPartBase<TScreenPart, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        protected ScreenPartBase(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            base.InputParameterType = builder.RegisterMetaType(typeof(TInput));
            base.ModelDataType = builder.RegisterMetaType(typeof(TData));
            base.ModelStateType = builder.RegisterMetaType(typeof(TState));

            builder.BuildNodes(builder.GetDeclaredMemberNodes(this));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            DescribePresenter(new PresenterBuilder<TScreenPart, TData, TState>(builder, this));
            builder.DescribeNodePresenters(this.ContentRoot);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TScreenPart, TData, TState> presenter);
    }
}
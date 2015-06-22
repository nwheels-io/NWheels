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

            builder.InstantiateDeclaredMemberNodes(this);
            builder.BuildNodes(this.ContentRoot);

            DescribePresenter(new PresenterBuilder<TScreenPart, TData, TState>(builder, this));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TScreenPart, TData, TState> presenter);
    }
}
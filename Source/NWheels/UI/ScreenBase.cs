using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public abstract class ScreenBase<TScreen, TInput, TData, TState> : UidlScreen, UidlBuilder.IBuildableUidlNode, IScreenWithInput<TInput>
        where TScreen : ScreenBase<TScreen, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        protected ScreenBase(string idName, UidlApplication parent)
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

            DescribePresenter(new PresenterBuilder<TScreen, TData, TState>(builder, this));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TScreen, TData, TState> presenter);
    }
}
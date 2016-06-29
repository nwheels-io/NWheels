using System.Linq;
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
            NavigatedHere = new UidlNotification<TInput>("NavigatedHere", this);
            Notifications.Add(this.NavigatedHere);
            base.NavigatedHere = this.NavigatedHere;
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
            DescribePresenter(new PresenterBuilder<TScreen, TData, TState>(builder, this));
            builder.DescribeNodePresenters(this.ContentRoot);
            PostDescribePresenter(new PresenterBuilder<TScreen, TData, TState>(builder, this));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TScreen, TData, TState> presenter);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void PostDescribePresenter(PresenterBuilder<TScreen, TData, TState> presenter)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public new UidlNotification<TInput> NavigatedHere { get; set; }
    }
}
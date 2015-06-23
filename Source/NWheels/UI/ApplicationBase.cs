using System.Linq;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    public abstract class ApplicationBase<TApp, TInput, TData, TState> : UidlApplication, UidlBuilder.IBuildableUidlNode
        where TApp : ApplicationBase<TApp, TInput, TData, TState>
        where TData : class
        where TState : class
    {
        protected ApplicationBase()
            : base(IdNameAsTypeMacro)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            base.Version = this.GetType().Assembly.GetName().Version.ToString();
            base.InputParameterType = builder.RegisterMetaType(typeof(TInput));
                
            var memberNodes = builder.InstantiateDeclaredMemberNodes(this);

            base.Screens.AddRange(memberNodes.OfType<UidlScreen>());
            base.ScreenParts.AddRange(memberNodes.OfType<UidlScreenPart>());

            builder.BuildNodes(base.ScreenParts.Cast<AbstractUidlNode>().ToArray());
            builder.BuildNodes(base.Screens.Cast<AbstractUidlNode>().ToArray());

            DescribePresenter(new PresenterBuilder<TApp, TData, TState>(builder, this));
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TApp, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void SetDefaultInitialScreen(UidlScreen screen)
        {
            base.DefaultInitialScreenQualifiedName = screen.QualifiedName;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationBase<TApp> : ApplicationBase<TApp, Empty.Input, Empty.Data, Empty.State>
        where TApp : ApplicationBase<TApp, Empty.Input, Empty.Data, Empty.State>
    {
    }
}

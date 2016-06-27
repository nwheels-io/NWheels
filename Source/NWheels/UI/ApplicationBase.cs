using System.Linq;
using System.Reflection;
using Autofac;
using NWheels.UI.Factories;
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
            var requiredApiContracts = this.GetType().GetCustomAttributes<RequireDomainApiAttribute>().Select(attr => attr.ContractType);
            base.RequiredDomainApis.AddRange(requiredApiContracts.Distinct());

            var requiredContextContracts = this.GetType().GetCustomAttributes<RequireDomainContextAttribute>().Select(attr => attr.ContractType);
            base.RequiredDomainContexts.AddRange(requiredContextContracts.Distinct());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            var appIdAttribute = this.GetType().GetCustomAttribute<AppIdAttribute>();

            if ( appIdAttribute != null && !string.IsNullOrWhiteSpace(appIdAttribute.Version) )
            {
                base.Version = appIdAttribute.Version;
            }
            else
            {
                base.Version = this.GetType().Assembly.GetName().Version.ToString();
            }

            NavigatedHere = new UidlNotification<TInput>("NavigatedHere", this);
            Notifications.Add(this.NavigatedHere);
            base.NavigatedHere = this.NavigatedHere;
            base.InputParameterType = builder.RegisterMetaType(typeof(TInput));
                
            var memberNodes = builder.GetDeclaredMemberNodes(this);

            base.Screens.AddRange(memberNodes.OfType<UidlScreen>());
            base.ScreenParts.AddRange(memberNodes.OfType<UidlScreenPart>());

            builder.BuildNodes(memberNodes);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            DescribePresenter(new PresenterBuilder<TApp, TData, TState>(builder, this));
            builder.DescribeNodePresenters(base.Screens.Cast<AbstractUidlNode>().ToArray());
            builder.DescribeNodePresenters(base.ScreenParts.Cast<AbstractUidlNode>().ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object CreateViewStateForCurrentUser(IComponentContext components)
        {
            return this.OnCreateViewStateForCurrentUser(components.Resolve<IViewModelObjectFactory>());
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public new UidlNotification<TInput> NavigatedHere { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool DesignMode { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DescribePresenter(PresenterBuilder<TApp, TData, TState> presenter);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual TState OnCreateViewStateForCurrentUser(IViewModelObjectFactory viewModelFactory)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void SetInitialScreen(UidlScreen ifNotAuthenticated, UidlScreen ifAuthenticated)
        {
            base.InitialScreenIfNotAuthenticated = ifNotAuthenticated;
            base.InitialScreenIfAuthenticated = ifAuthenticated;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string GetIdNameFromType()
        {
            var appIdAttribute = this.GetType().GetCustomAttribute<AppIdAttribute>();

            if ( appIdAttribute != null )
            {
                return appIdAttribute.IdName;
            }
            else
            {
                return base.GetIdNameFromType();
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ApplicationBase<TApp> : ApplicationBase<TApp, Empty.Input, Empty.Data, Empty.State>
        where TApp : ApplicationBase<TApp, Empty.Input, Empty.Data, Empty.State>
    {
    }
}

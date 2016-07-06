using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Extensions;
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

            var declaredMemberNodes = builder.GetDeclaredMemberNodes(this);

            if (base.PopupContents == null)
            {
                base.PopupContents = new List<WidgetUidlNode>();
            }
            base.PopupContents.AddRange(declaredMemberNodes.OfType<WidgetUidlNode>().Where(widget => widget.IsPopupContent));
            
            builder.BuildNodes(declaredMemberNodes);
            OnBuild(builder);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
            DescribePresenter(new PresenterBuilder<TScreenPart, TData, TState>(builder, this));
            builder.DescribeNodePresenters(this.ContentRoot);
            builder.DescribeNodePresenters(this.PopupContents.Cast<AbstractUidlNode>().ToArray());

            this.Text = this.Text.TrimTail("ScreenPart");

            if (PostDescribingPresenter != null)
            {
                PostDescribingPresenter(new PresenterBuilder<TScreenPart, TData, TState>(builder, this));
            }

            base.ApplyExtensions(builder);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(base.PopupContents.SelectMany(popup => popup.GetTranslatables()));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<PresenterBuilder<TScreenPart, TData, TState>> PostDescribingPresenter;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void DescribePresenter(PresenterBuilder<TScreenPart, TData, TState> presenter)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnBuild(UidlBuilder builder)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public new UidlNotification<TInput> NavigatedHere { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Services;
using Action = System.Action;
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    [Export(typeof(IApplicationExplorer))]
    public class ApplicationExplorerViewModel : 
        Tool, 
        IApplicationExplorer, 
        IHandle<AppOpenedMessage>,
        IHandle<AppClosedMessage>,
        IHandle<AppStateChangedMessage>
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationExplorerViewModel(IEventAggregator events, IShell shell, IApplicationControllerService controller)
        {
            _controller = controller;
            this.ExplorerItems = new ObservableCollection<ExplorerItem>();

            events.Subscribe(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return "Application Explorer"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool ShouldReopenOnStart
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppOpenedMessage>.Handle(AppOpenedMessage message)
        {
            Execute.OnUIThreadAsync(() => AddApplicationItem(message.App));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppClosedMessage>.Handle(AppClosedMessage message)
        {
            Execute.OnUIThreadAsync(() => RemoveApplicationItem(message.App));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppStateChangedMessage>.Handle(AppStateChangedMessage message)
        {
            Execute.OnUIThreadAsync(() => UpdateApplicationItem(message.App));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObservableCollection<ExplorerItem> ExplorerItems { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddApplicationItem(ApplicationController app)
        {
            ExplorerItems.Add(new ApplicationItem(app));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RemoveApplicationItem(ApplicationController app)
        {
            var itemToRemove = ExplorerItems
                .OfType<ApplicationItem>()
                .Single(item => item.Controller == app);

            ExplorerItems.Remove(itemToRemove);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateApplicationItem(ApplicationController app)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum ExplorerItemSize
        {
            Smallest = 12,
            Small = 13,
            Medium = 14,
            Large = 15,
            Largest = 16
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExplorerItem
        {
            private readonly string _text;
            private readonly ImageSource _icon;
            private readonly string _tooltipText;
            private readonly Func<IEnumerable<ExplorerItem>> _subItemFactory;
            private readonly Action _selectCallback;
            private readonly Action _doubleClickCallback;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExplorerItem(
                string text,
                ImageSource icon = null,
                string tooltipText = null,
                Func<IEnumerable<ExplorerItem>> subItemFactory = null,
                Action selectCallbeck = null,
                Action doubleClickCallbck = null)
            {
                _text = text;
                _icon = icon;
                _tooltipText = tooltipText;
                _subItemFactory = subItemFactory;
                _selectCallback = selectCallbeck;
                _doubleClickCallback = doubleClickCallbck;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void DoubleClick()
            {
                if ( _doubleClickCallback != null )
                {
                    _doubleClickCallback();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void Select()
            {
                if ( _selectCallback != null)
                {
                    _selectCallback();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string Text
            {
                get { return _text; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual ImageSource Icon
            {
                get { return _icon; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string TooltipText
            {
                get { return _tooltipText; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual IEnumerable<ExplorerItem> SubItems
            {
                get { return _subItemFactory(); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int FontSize
            {
                get
                {
                    return (int)ItemSize;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ExplorerItemSize ItemSize { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ControllerItem<TController> : ExplorerItem
            where TController : ControllerBase
        {
            private readonly TController _controller;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ControllerItem(TController controller, ImageSource icon, ExplorerItemSize itemSize)
                : base(controller.DisplayName, icon)
            {
                _controller = controller;
                base.ItemSize = itemSize;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TController Controller
            {
                get { return _controller; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ApplicationItem : ControllerItem<ApplicationController>
        {
            public ApplicationItem(ApplicationController app)
                : base(app, AppIcons.AppExplorerIconApplication, ExplorerItemSize.Largest)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItem> SubItems
            {
                get
                {
                    return Controller.SubControllers.Select(env => new EnvironmentItem(env));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EnvironmentItem : ControllerItem<EnvironmentController>
        {
            public EnvironmentItem(EnvironmentController env)
                : base(env, AppIcons.AppExplorerIconEnvironment, ExplorerItemSize.Large)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItem> SubItems
            {
                get
                {
                    return Controller.SubControllers.Select(node => new NodeItem(node));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NodeItem : ControllerItem<NodeController>
        {
            public NodeItem(NodeController node)
                : base(node, AppIcons.AppExplorerIconNode, ExplorerItemSize.Medium)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItem> SubItems
            {
                get
                {
                    return Controller.SubControllers.Select(instance => new NodeInstanceItem(instance));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NodeInstanceItem : ControllerItem<NodeInstanceController>
        {
            public NodeInstanceItem(NodeInstanceController instance)
                : base(instance, AppIcons.AppExplorerIconNodeInstance, ExplorerItemSize.Small)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //public override IEnumerable<ExplorerItem> SubItems
            //{
            //    get
            //    {

            //    }
            //}
        }
    }
}

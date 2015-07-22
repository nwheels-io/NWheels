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
using NWheels.Extensions;
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
        private readonly IApplicationControllerService _controllerService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationExplorerViewModel(IEventAggregator events, IShell shell, IApplicationControllerService controllerService)
        {
            _controllerService = controllerService;
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
            ExplorerItems.Add(new ApplicationItem(this, app));
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

        public class ExplorerItem : PropertyChangedBase
        {
            private readonly ExplorerItem _parent;
            private readonly string _text;
            private readonly ImageSource _icon;
            private readonly string _tooltipText;
            private readonly Func<IEnumerable<ExplorerItem>> _subItemFactory;
            private readonly Action _selectCallback;
            private readonly Action _doubleClickCallback;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExplorerItem(
                ExplorerItem parent,
                string text,
                ImageSource icon = null,
                string tooltipText = null,
                Func<IEnumerable<ExplorerItem>> subItemFactory = null,
                Action selectCallbeck = null,
                Action doubleClickCallbck = null)
            {
                _parent = parent;
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

            public virtual string FullPath
            {
                get
                {
                    if ( _parent != null )
                    {
                        return _parent.FullPath + " / " + this._text;
                    }
                    else
                    {
                        return this._text;
                    }
                }
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

            protected ControllerItem(ExplorerItem parent, TController controller, ImageSource icon, ExplorerItemSize itemSize)
                : base(parent, controller.DisplayName, icon)
            {
                _controller = controller;
                _controller.CurrentStateChanged += OnControllerStateChanged;
                base.ItemSize = itemSize;
            }

            void OnControllerStateChanged(object sender, ControllerStateEventArgs e)
            {
                NotifyOfPropertyChange(() => this.CurrentState);
                NotifyOfPropertyChange(() => this.CanStart);
                NotifyOfPropertyChange(() => this.CanStop);
                NotifyOfPropertyChange(() => this.CanClose);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void Start()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void Stop()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void Close()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual bool CanStart
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public virtual bool CanStop
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public virtual bool CanClose
            {
                get { return false; }
            }

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public virtual bool CanStart()
            //{
            //    return false;
            //}

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public virtual bool CanStop()
            //{
            //    return false;
            //}

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public virtual bool CanClose()
            //{
            //    return false;
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TController Controller
            {
                get { return _controller; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeState CurrentState
            {
                get { return _controller.CurrentState; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ApplicationItem : ControllerItem<ApplicationController>
        {
            private readonly ApplicationExplorerViewModel _ownerViewModel;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApplicationItem(ApplicationExplorerViewModel ownerViewModel, ApplicationController app)
                : base(null, app, AppIcons.AppExplorerIconApplication, ExplorerItemSize.Largest)
            {
                _ownerViewModel = ownerViewModel;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Start()
            {
                Task.Run(() => Controller.LoadAndActivate());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Stop()
            {
                Task.Run(() => Controller.DeactivateAndUnload());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Close()
            {
                Task.Run(() => _ownerViewModel._controllerService.Close(this.Controller));
            }

            public override bool CanStart
            {
                get { return Controller.CanLoad(); }
            }
            public override bool CanStop
            {
                get { return Controller.CanDeactivate(); }
            }
            public override bool CanClose
            {
                get { return true; }
            }

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public override bool CanStart()
            //{
            //    return Controller.CanLoad();
            //}

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public override bool CanStop()
            //{
            //    return Controller.CanDeactivate();
            //}

            ////-------------------------------------------------------------------------------------------------------------------------------------------------

            //public override bool CanClose()
            //{
            //    return true;
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItem> SubItems
            {
                get
                {
                    return Controller.SubControllers.Select(env => new EnvironmentItem(this, env));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EnvironmentItem : ControllerItem<EnvironmentController>
        {
            public EnvironmentItem(ApplicationItem parent, EnvironmentController env)
                : base(parent, env, AppIcons.AppExplorerIconEnvironment, ExplorerItemSize.Large)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItem> SubItems
            {
                get
                {
                    return Controller.SubControllers.Select(node => new NodeItem(this, node));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NodeItem : ControllerItem<NodeController>
        {
            public NodeItem(EnvironmentItem parent, NodeController node)
                : base(parent, node, AppIcons.AppExplorerIconNode, ExplorerItemSize.Medium)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItem> SubItems
            {
                get
                {
                    return Controller.SubControllers.Select(instance => new NodeInstanceItem(this, instance));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NodeInstanceItem : ControllerItem<NodeInstanceController>
        {
            public NodeInstanceItem(NodeItem parent, NodeInstanceController instance)
                : base(parent, instance, AppIcons.AppExplorerIconNodeInstance, ExplorerItemSize.Small)
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

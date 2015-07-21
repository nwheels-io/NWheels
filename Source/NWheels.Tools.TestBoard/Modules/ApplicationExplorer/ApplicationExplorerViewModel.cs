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

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    [Export(typeof(IApplicationExplorer))]
    public class ApplicationExplorerViewModel : 
        Tool, 
        IApplicationExplorer, 
        IHandle<AppLoadedMessage>,
        IHandle<AppUnloadedMessage>
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

        void IHandle<AppLoadedMessage>.Handle(AppLoadedMessage message)
        {
            Execute.OnUIThreadAsync(() => AddApplicationItem(message.BootConfig));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppUnloadedMessage>.Handle(AppUnloadedMessage message)
        {
            Execute.OnUIThreadAsync(() => RemoveApplicationItem(message.BootConfig));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObservableCollection<ExplorerItem> ExplorerItems { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddApplicationItem(BootConfiguration bootConfig)
        {
            ExplorerItems.Add(new ApplicationItem(bootConfig));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RemoveApplicationItem(BootConfiguration bootConfig)
        {
            var itemsToRemove = ExplorerItems
                .OfType<ApplicationItem>()
                .Where(item => item.BootConfig.ApplicationName == bootConfig.ApplicationName)
                .ToArray();

            foreach ( var item in itemsToRemove )
            {
                ExplorerItems.Remove(item);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum ExplorerItemSize
        {
            Small = 12,
            Medium = 14,
            Large = 16
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

        public class ApplicationItem : ExplorerItem
        {
            private readonly BootConfiguration _bootConfig;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApplicationItem(BootConfiguration bootConfig)
                : base(text: FormatItemText(bootConfig), icon: AppIcons.Get("AppExplorerIconApplication"))
            {
                _bootConfig = bootConfig;
                base.ItemSize = ExplorerItemSize.Large;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //public override IEnumerable<ExplorerItem> SubItems
            //{
            //    get
            //    {
                                                     
            //    }
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BootConfiguration BootConfig
            {
                get { return _bootConfig; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static string FormatItemText(BootConfiguration bootConfig)
            {
                return string.Format(
                    "{0}/{1} @ {2}({3})", 
                    bootConfig.ApplicationName, bootConfig.NodeName, bootConfig.EnvironmentName, bootConfig.EnvironmentType);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using NWheels.Tools.TestBoard.Services;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    [Export(typeof(IApplicationExplorer))]
    public class ApplicationExplorerViewModel : Tool, IApplicationExplorer
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationExplorerViewModel(IShell shell, IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationState CurrentState
        {
            get
            {
                return _controller.CurrentState;
            }
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

        public abstract class ExplorerItemBase
        {
            public virtual void DoubleClick()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual void Select()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string Text
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual ImageSource Icon
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual string TooltipText
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual IEnumerable<ExplorerItemBase> SubItems
            {
                get { return new ExplorerItemBase[0]; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class SimpleExplorerItemBase : ExplorerItemBase
        {
            private readonly string _text;
            private readonly ImageSource _icon;
            private readonly string _tooltipText;
            private readonly Func<IEnumerable<ExplorerItemBase>> _subItemFactory;
            private readonly Action _selectCallback;
            private readonly Action _doubleClickCallback;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected SimpleExplorerItemBase(
                string text,
                ImageSource icon = null,
                string tooltipText = null,
                Func<IEnumerable<ExplorerItemBase>> subItemFactory = null,
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

            public override void DoubleClick()
            {
                if ( _doubleClickCallback != null )
                {
                    _doubleClickCallback();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Select()
            {
                if ( _selectCallback != null)
                {
                    _selectCallback();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string Text
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override ImageSource Icon
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string TooltipText
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<ExplorerItemBase> SubItems
            {
                get { return new ExplorerItemBase[0]; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class ApplicationBase : UIElementBase, IApplication
    {
        private readonly List<ScreenBase> _screens;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ApplicationBase()
        {
            _screens = new List<ScreenBase>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IApplication

        public string Icon { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Copyright { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IScreen IApplication.InitialScreen
        {
            get { return this.InitialScreen; }
            set { this.InitialScreen = (ScreenBase)value; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<ScreenBase> Screens
        {
            get { return _screens; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ScreenBase InitialScreen { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void AddScreen(ScreenBase screen)
        {
            _screens.Add(screen);
        }
    }
}

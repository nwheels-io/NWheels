using System.Windows.Media;
using Caliburn.Micro;
using NWheels.Stacks.UI.WpfCaliburnAvalon.Wpf;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.Caliburn
{
    public abstract class PanelViewModelBase : PropertyChangedBase
    {
        private string _title;
        private ImageSource _icon;
        private bool _isActive;
        private bool _isVisible = true;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PanelViewModelBase()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PanelViewModelBase(string title = null, string iconName = null)
        {
            _title = title;

            if (!string.IsNullOrEmpty(iconName))
            {
                _icon = ResourceHelper.LoadBitmap(iconName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Saved()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool IsFileContent
        {
            get { return false; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string Filename
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string FilePath { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string FileContent
        {
            get
            {
                return string.Empty;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string ContentId
        {
            get
            {
                return GetType().ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                NotifyOfPropertyChange(() => IsActive);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
            private set
            {
                _icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string IconName
        {
            set
            {
                Icon = ResourceHelper.LoadBitmap(value);
            }
        }
    }
}

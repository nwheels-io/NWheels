using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    public class TreeNodeItem<T> : DependencyObject, ITreeNodeItemEventHandlers where T : class, IEnumerable<T>
    {
        private readonly ObservableCollection<TreeNodeItem<T>> _visibleItems;
        private readonly int _depth = 0;

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public TreeNodeItem(T data, ObservableCollection<TreeNodeItem<T>> visibleItems, TreeNodeItem<T> parentNode, TreeNodeItem<T> prevSiblingNode)
        {
            _visibleItems = visibleItems;

            this.Data = data;
            this.ParentNode = parentNode;
            this.PrevSiblingNode = prevSiblingNode;

            if ( prevSiblingNode != null )
            {
                prevSiblingNode.NextSiblingNode = this;
            }
            else if ( parentNode != null )
            {
                parentNode.FirstChildNode = this;
            }

            _depth = (parentNode != null ? parentNode.Depth + 1 : 0);

            if ( data.Any() )
            {
                this.IsExpanded = false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Toggle()
        {
            IsExpanded = !IsExpanded;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void ItemPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ( Data.Any() )
            {
                switch ( e.Key )
                {
                    case Key.Right:
                        IsExpanded = true;
                        break;
                    case Key.Left:
                        IsExpanded = false;
                        break;
                    case Key.Space:
                        Toggle();
                        break;
                    default:
                        return;
                }

                e.Handled = true;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void ItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ( Data.Any() )
            {
                Toggle();
                e.Handled = true;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public T Data
        {
            get
            {
                return (T)GetValue(DataProperty);
            }
            set
            {
                SetValue(DataProperty, value);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsExpanded
        {
            get
            {
                return (bool)GetValue(IsExpandedProperty);
            }
            set
            {
                var oldValue = (bool)GetValue(IsExpandedProperty);
                SetValue(IsExpandedProperty, value);

                if ( value )
                {
                    if ( !oldValue )
                    {
                        ShowChildrenItems();
                    }

                    ImagePath = "pack://application:,,,/Resources/IconExpanded.png";
                    HoverImagePath = "pack://application:,,,/Resources/IconExpanded.png";
                }
                else
                {
                    if ( oldValue )
                    {
                        HideChildrenItems();
                    }

                    ImagePath = "pack://application:,,,/Resources/IconCollapsed.png";
                    HoverImagePath = "pack://application:,,,/Resources/IconCollapsed.png";
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Should be bound to ListView item container
        /// </summary>
        public bool IsSelected { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public int Depth
        {
            get
            {
                return _depth;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public int IndentSize
        {
            get
            {
                return (_depth * 20);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public string ImagePath
        {
            get
            {
                return (string)GetValue(ImagePathProperty);
            }
            private set
            {
                SetValue(ImagePathProperty, value);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public string HoverImagePath
        {
            get
            {
                return (string)GetValue(HoverImagePathProperty);
            }
            set
            {
                SetValue(HoverImagePathProperty, value);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public TreeNodeItem<T> ParentNode { get; private set; }
        public TreeNodeItem<T> FirstChildNode { get; private set; }
        public TreeNodeItem<T> PrevSiblingNode { get; private set; }
        public TreeNodeItem<T> NextSiblingNode { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateChildrenItems()
        {
            TreeNodeItem<T> lastInsertedChild = null;
            var childrenData = this.Data.ToArray();

            for ( int i = 0 ; i < childrenData.Length ; i++ )
            {
                lastInsertedChild = new TreeNodeItem<T>(childrenData[i], _visibleItems, this, lastInsertedChild);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void HideChildrenItems()
        {
            var thisItemIndex = _visibleItems.IndexOf(this);

            while ( _visibleItems.Count > thisItemIndex + 1 && _visibleItems[thisItemIndex + 1] != NextSiblingNode )
            {
                _visibleItems.RemoveAt(thisItemIndex + 1);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void ShowChildrenItems()
        {
            var startItemIndex = _visibleItems.IndexOf(this) + 1;
            ShowChildrenItems(ref startItemIndex);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void ShowChildrenItems(ref int startItemIndex)
        {
            if ( FirstChildNode == null )
            {
                CreateChildrenItems();
            }

            for ( var child = FirstChildNode ; child != null ; child = child.NextSiblingNode )
            {
                _visibleItems.Insert(startItemIndex, child);
                startItemIndex++;

                if ( child.IsExpanded )
                {
                    child.ShowChildrenItems(ref startItemIndex);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data",
            typeof(T),
            typeof(TreeNodeItem<T>),
            new FrameworkPropertyMetadata(defaultValue: null));

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded",
            typeof(bool),
            typeof(TreeNodeItem<T>),
            new FrameworkPropertyMetadata(defaultValue: false));

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(
            "ImagePath",
            typeof(string),
            typeof(TreeNodeItem<T>),
            new FrameworkPropertyMetadata(defaultValue: ""));

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly DependencyProperty HoverImagePathProperty = DependencyProperty.Register(
            "HoverImagePath",
            typeof(string),
            typeof(TreeNodeItem<T>),
            new FrameworkPropertyMetadata(defaultValue: ""));
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITreeNodeItemEventHandlers
    {
        void ItemPreviewKeyDown(object sender, KeyEventArgs e);
        void ItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e);
    }
}

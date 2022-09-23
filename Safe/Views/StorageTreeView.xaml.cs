using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EdlinSoftware.Safe.ViewModels;

namespace EdlinSoftware.Safe.Views
{
    /// <summary>
    /// Interaction logic for StorageTreeView.xaml
    /// </summary>
    public partial class StorageTreeView : UserControl
    {
        private Point _startPoint;
        private ItemTreeViewModel? _itemToDrag;

        public StorageTreeView()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);

            _itemToDrag = GetItemViewModel(e.OriginalSource);
        }

        private void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _itemToDrag != null)
            {
                var mousePos = e.GetPosition(null);
                var diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = (TreeView)sender;

                    var dragData = new DataObject(_itemToDrag);
                    DragDrop.DoDragDrop(treeView, dragData, DragDropEffects.Move);
                }
            }
        }

        private void UIElement_CheckDrag(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var uiElement = (UIElement)sender;

            var element = uiElement.InputHitTest(e.GetPosition(uiElement));

            var itemUnderMouse = GetItemViewModel(element);

            if (itemUnderMouse == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            if (IsChild(itemUnderMouse))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            e.Effects = DragDropEffects.Move;
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var uiElement = (UIElement)sender;

            var element = uiElement.InputHitTest(e.GetPosition(uiElement));

            var itemUnderMouse = GetItemViewModel(element);

            if (itemUnderMouse == null) return;

            if (IsChild(itemUnderMouse)) return;

            _itemToDrag!.MoveTo(itemUnderMouse);
        }

        private bool IsChild(ItemTreeViewModel? item)
        {
            while (item != null)
            {
                if (ReferenceEquals(item, _itemToDrag)) return true;

                item = item.Parent;
            }

            return false;
        }

        private ItemTreeViewModel? GetItemViewModel(object uiElement)
        {
            var depElement = uiElement as DependencyObject;

            while (true)
            {
                var frElement = depElement as FrameworkElement;
                if (frElement == null) break;

                var item = frElement.DataContext as ItemTreeViewModel;
                if (item != null)
                {
                    return item;
                }

                depElement = VisualTreeHelper.GetParent(frElement);
            }

            return null;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _itemToDrag = null;
        }

    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EdlinSoftware.Safe.ViewModels;

namespace EdlinSoftware.Safe.Views
{
    /// <summary>
    /// Interaction logic for CreateItemView.xaml
    /// </summary>
    public partial class CreateItemView : UserControl
    {
        private Point _startPoint;
        private FieldViewModel? _fieldToDrag;

        public CreateItemView()
        {
            InitializeComponent();
        }


        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);

            _fieldToDrag = GetFieldViewModel(e.OriginalSource);
        }

        private void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _fieldToDrag?.ContainingCollection != null)
            {
                var mousePos = e.GetPosition(null);
                var diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var list = (DependencyObject)sender;

                    var dragData = new DataObject(_fieldToDrag);
                    DragDrop.DoDragDrop(list, dragData, DragDropEffects.Move);
                }
            }
        }

        private void UIElement_CheckDrag(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var uiElement = (UIElement)sender;

            var element = uiElement.InputHitTest(e.GetPosition(uiElement));

            var fieldUnderMouse = GetFieldViewModel(element);

            if (fieldUnderMouse == null)
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

            var fieldUnderMouse = GetFieldViewModel(element);

            if (fieldUnderMouse == null) return;

            var containingCollection = _fieldToDrag!.ContainingCollection;

            var initialIndex = containingCollection!.IndexOf(_fieldToDrag);
            var targetIndex = containingCollection.IndexOf(fieldUnderMouse);

            containingCollection.Move(initialIndex, targetIndex);
        }

        private FieldViewModel? GetFieldViewModel(object uiElement)
        {
            var depElement = uiElement as DependencyObject;

            while (true)
            {
                var frElement = depElement as FrameworkElement;
                if (frElement == null) break;

                var item = frElement.DataContext as FieldViewModel;
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
            _fieldToDrag = null;
        }
    }
}

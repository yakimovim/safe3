using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace EdlinSoftware.Safe.Views.Behaviors
{
    public class BindableSelectedItemsBehavior : Behavior<ListView>
    {
        #region SelectedItem Property

        public IReadOnlyList<object> SelectedItems
        {
            get => (IReadOnlyList<object>) GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                nameof(SelectedItems), 
                typeof(IReadOnlyList<object>), 
                typeof(BindableSelectedItemsBehavior), 
                new UIPropertyMetadata(null, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ListBoxItem item)
            {
                item.SetValue(ListBoxItem.IsSelectedProperty, true);
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnListViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged -= OnListViewSelectedItemChanged;
            }
        }

        private void OnListViewSelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems = AssociatedObject.SelectedItems.OfType<object>().ToArray();
        }
    }
}

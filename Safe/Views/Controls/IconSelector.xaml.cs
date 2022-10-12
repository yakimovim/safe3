using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EdlinSoftware.Safe.ViewModels.Controls;

namespace EdlinSoftware.Safe.Views.Controls
{
    /// <summary>
    /// Interaction logic for IconSelector.xaml
    /// </summary>
    public partial class IconSelector : UserControl
    {
        public static readonly DependencyProperty IconIdProperty = DependencyProperty
            .Register(
                nameof(IconId),
                typeof(string),
                typeof(IconSelector),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIconIdentifierChanged)
            );

        private static void OnIconIdentifierChanged(
            DependencyObject d, 
            DependencyPropertyChangedEventArgs e)
        {
            var selector = d as IconSelector;
            if(selector == null) return;

            if(selector.DataContext is IconSelectorViewModel vm)
                vm.IconId = e.NewValue as string;
        }

        public string? IconId
        {
            get => (string?)GetValue(IconIdProperty);
            set => SetValue(IconIdProperty, value);
        }

        public IconSelector()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            if (DataContext is IconSelectorViewModel vm)
            {
                vm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is IconSelectorViewModel vmOld)
            {
                vmOld.PropertyChanged -= OnViewModelPropertyChanged;
            }
            if (e.NewValue is IconSelectorViewModel vmNew)
            {
                vmNew.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IconId))
            {
                IconId = ((IconSelectorViewModel) DataContext).IconId;
            }
        }
    }
}

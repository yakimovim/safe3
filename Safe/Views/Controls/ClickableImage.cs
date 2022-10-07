using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EdlinSoftware.Safe.Views.Controls;

public class ClickableImage : Image
{
    public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
        nameof(ClickCommand),
        typeof(ICommand),
        typeof(ClickableImage),
        new FrameworkPropertyMetadata(null)
    );

    public ICommand? ClickCommand
    {
        get => (ICommand?) GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    public ClickableImage()
    {
        MouseLeftButtonUp += (_, _) =>
        {
            var command = ClickCommand;

            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
            }
        };
    }
}
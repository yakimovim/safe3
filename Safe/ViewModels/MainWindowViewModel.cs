using System.Windows;
using Prism.Commands;

namespace EdlinSoftware.Safe.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        ExitCommand = new DelegateCommand(OnExit);
    }

    private void OnExit()
    {
        Application.Current.Shutdown(0);
    }

    public DelegateCommand ExitCommand { get; }
}
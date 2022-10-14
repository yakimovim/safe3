using System;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public class ConfirmationDialogViewModel : ViewModelBase, IDialogAware
{
    internal static readonly string TitlePropertyName = "Title";

    public ConfirmationDialogViewModel()
    {
        NoCommand = new DelegateCommand(OnNo);
        YesCommand = new DelegateCommand(OnYes);
    }

    private void OnYes()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
    }

    private void OnNo()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.No));
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        Title = parameters.GetValue<string>(TitlePropertyName);
    }

    public string Title { get; private set; } = string.Empty;

    public event Action<IDialogResult>? RequestClose;

    public DelegateCommand NoCommand { get; }
    public DelegateCommand YesCommand { get; }
}

public static class ConfirmationDialogExtensions
{
    public static void ShowConfirmationDialog(this IDialogService dialogService,
        string title,
        Action<ButtonResult> action)
    {
        var p = new DialogParameters { { ConfirmationDialogViewModel.TitlePropertyName, title } };

        dialogService.ShowDialog(nameof(ConfirmationDialog), p, res =>
        {
            action.Invoke(res.Result);
        });
    }
}
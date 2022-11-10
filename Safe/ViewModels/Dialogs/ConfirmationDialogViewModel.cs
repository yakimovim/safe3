using System;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class ConfirmationDialogViewModel : ViewModelBase, IDialogAware
{
    internal static readonly string TitlePropertyName = "Title";

    [RelayCommand]
    private void No()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.No));
    }

    [RelayCommand]
    private void Yes()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
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
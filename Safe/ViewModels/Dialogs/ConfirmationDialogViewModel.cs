using System;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class ConfirmationDialogViewModel : ObservableDialogBase
{
    internal static readonly string TitlePropertyName = "Title";

    [RelayCommand]
    private void No()
    {
        RequestDialogClose(ButtonResult.No);
    }

    [RelayCommand]
    private void Yes()
    {
        RequestDialogClose(ButtonResult.Yes);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        Title = parameters.GetValue<string>(TitlePropertyName);
    }
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
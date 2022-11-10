using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class FieldsDialogViewModel : ObservableObject, IDialogAware
{
    public const string FieldsPropertyName = "Fields";

    public FieldsDialogViewModel()
    {
        SelectedFields.CollectionChanged += (_, _) =>
        {
            SelectFieldsCommand.NotifyCanExecuteChanged();
            ClearSelectedFieldsCommand.NotifyCanExecuteChanged();
        };
    }

    private bool CanClearSelectedFields() => (SelectedFields?.Count ?? 0) > 0;

    [RelayCommand(CanExecute = nameof(CanClearSelectedFields))]
    private void ClearSelectedFields()
    {
        foreach (var field in SelectedFields)
        {
            field.ContainingCollection = null;
        }
        SelectedFields.Clear();
    }

    private bool CanRemoveSelectedFields() => (CurrentSelectedFields?.Count ?? 0) > 0;

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedFields))]
    private void RemoveSelectedFields()
    {
        foreach (var field in CurrentSelectedFields.OfType<FieldViewModel>())
        {
            field.ContainingCollection = null;
            SelectedFields.Remove(field);
        }
    }

    private bool CanAddSelectedFields() => (CurrentAvailableFields?.Count ?? 0) > 0;

    [RelayCommand(CanExecute = nameof(CanAddSelectedFields))]
    private void OnAddSelectedFields()
    {
        foreach (var availableField in CurrentAvailableFields.OfType<FieldViewModel>())
        {
            var field = availableField.MakeCopy();
            field.ContainingCollection = SelectedFields;
            SelectedFields.Add(field);
        }
    }

    private bool CanSelectFields() => (SelectedFields?.Count ?? 0) > 0;

    [RelayCommand(CanExecute = nameof(CanSelectFields))]
    private void SelectFields()
    {
        var p = new DialogParameters
        {
            { FieldsPropertyName, SelectedFields }
        };

        RequestClose?.Invoke(new DialogResult(ButtonResult.OK, p));
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        SelectedFields.Clear();
    }

    public string Title { get; } = (string) Application.Current.Resources["AddFieldsDialogTitle"];

    public event Action<IDialogResult>? RequestClose;

    public ObservableCollection<FieldViewModel> AvailableFields { get; } = new()
    {
        new TextFieldViewModel(new TextField { Name = "URL:" }),
        new TextFieldViewModel(new TextField { Name = "Email:" }),
        new TextFieldViewModel(new TextField { Name = "Login:" }),
        new PasswordFieldViewModel(new PasswordField { Name = "Password:" }),
        new TextFieldViewModel(new TextField { Name = "Text" }),
        new PasswordFieldViewModel(new PasswordField { Name = "Password" })
    };

    [ObservableProperty]
    private ObservableCollection<FieldViewModel> _selectedFields = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedFieldsCommand))]
    private IReadOnlyList<object> _currentAvailableFields = Array.Empty<object>();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFieldsCommand))]
    private IReadOnlyList<object> _currentSelectedFields = new List<object>();
}

public static class FieldsDialogExtensions
{
    public static void ShowAddFieldsDialog(this IDialogService dialogService, 
        ObservableCollection<FieldViewModel> fields,
        EventHandler<FieldViewModel> fieldDeleteHandler)
    {
        dialogService.ShowDialog(nameof(FieldsDialog), new DialogParameters(), result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                var fieldsToAdd = result.Parameters.GetValue<IReadOnlyCollection<FieldViewModel>>(FieldsDialogViewModel.FieldsPropertyName);

                foreach (var field in fieldsToAdd)
                {
                    field.ContainingCollection = fields;
                    field.Deleted += fieldDeleteHandler;
                }

                fields.AddRange(fieldsToAdd);
            }
        });

    }
}
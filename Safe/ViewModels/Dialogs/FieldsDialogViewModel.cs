using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class FieldsDialogViewModel : ObservableDialogBase
{
    public const string FieldsPropertyName = "Fields";

    public FieldsDialogViewModel()
    {
        SetTitleFromResource("AddFieldsDialogTitle");

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

        RequestDialogClose(ButtonResult.OK, p);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestDialogClose(ButtonResult.Cancel);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        SelectedFields.Clear();
    }

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
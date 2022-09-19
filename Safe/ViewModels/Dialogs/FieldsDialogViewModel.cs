using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public class FieldsDialogViewModel : ViewModelBase, IDialogAware
{
    public const string FieldsPropertyName = "Fields";

    public FieldsDialogViewModel()
    {
        CancelCommand = new DelegateCommand(OnCancel);
        SelectFieldsCommand = new DelegateCommand(OnSelectFields, CanSelectFields);
        AddSelectedFieldCommand = new DelegateCommand(OnAddSelectedFields, CanAddSelectedFields)
            .ObservesProperty(() => CurrentAvailableFields);
        RemoveSelectedFieldCommand = new DelegateCommand(OnRemoveSelectedFields, CanRemoveSelectedFields)
            .ObservesProperty(() => CurrentSelectedFields);
        ClearSelectedFieldsCommand = new DelegateCommand(OnClearSelectedFields, CanClearSelectedFields);

        SelectedFields.CollectionChanged += (_, _) =>
        {
            SelectFieldsCommand.RaiseCanExecuteChanged();
            ClearSelectedFieldsCommand.RaiseCanExecuteChanged();
        };
    }

    private bool CanClearSelectedFields() => SelectedFields.Count > 0;

    private void OnClearSelectedFields()
    {
        foreach (var field in SelectedFields)
        {
            field.ContainingCollection = null;
        }
        SelectedFields.Clear();
    }

    private bool CanRemoveSelectedFields() => CurrentSelectedFields.Count > 0;

    private void OnRemoveSelectedFields()
    {
        foreach (var field in CurrentSelectedFields.OfType<FieldViewModel>())
        {
            field.ContainingCollection = null;
            SelectedFields.Remove(field);
        }
    }

    private bool CanAddSelectedFields() => CurrentAvailableFields.Count > 0;

    private void OnAddSelectedFields()
    {
        foreach (var availableField in CurrentAvailableFields.OfType<FieldViewModel>())
        {
            var field = availableField.MakeCopy();
            field.ContainingCollection = SelectedFields;
            SelectedFields.Add(field);
        }
    }

    private bool CanSelectFields() => SelectedFields.Count > 0;

    private void OnSelectFields()
    {
        var p = new DialogParameters
        {
            { FieldsPropertyName, SelectedFields }
        };

        RequestClose?.Invoke(new DialogResult(ButtonResult.OK, p));
    }

    private void OnCancel()
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

    public string Title { get; } = "Select fields";

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

    public ObservableCollection<FieldViewModel> SelectedFields { get; } = new();

    private IReadOnlyList<object> _currentAvailableFields = new List<object>();
    public IReadOnlyList<object> CurrentAvailableFields
    {
        get => _currentAvailableFields;
        set => SetProperty(ref _currentAvailableFields, value ?? new List<object>());
    }

    private IReadOnlyList<object> _currentSelectedFields = new List<object>();
    public IReadOnlyList<object> CurrentSelectedFields
    {
        get => _currentSelectedFields;
        set => SetProperty(ref _currentSelectedFields, value ?? new List<object>());
    }

    public DelegateCommand SelectFieldsCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AddSelectedFieldCommand { get; }
    public DelegateCommand RemoveSelectedFieldCommand { get; }
    public DelegateCommand ClearSelectedFieldsCommand { get; }
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
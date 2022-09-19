using System;
using System.Collections.ObjectModel;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public class FieldsDialogViewModel : ViewModelBase, IDialogAware
{
    public FieldsDialogViewModel()
    {
        CancelCommand = new DelegateCommand(OnCancel);
        SelectFieldsCommand = new DelegateCommand(OnSelectFields, CanSelectFields);
        AddSelectedFieldCommand = new DelegateCommand(OnAddSelectedField, CanAddSelectedField)
            .ObservesProperty(() => CurrentAvailableField);
        RemoveSelectedFieldCommand = new DelegateCommand(OnRemoveSelectedField, CanRemoveSelectedField)
            .ObservesProperty(() => CurrentSelectedField);
        ClearSelectedFieldsCommand = new DelegateCommand(OnClearSelectedFields, CanClearSelectedFields);

        SelectedFields.CollectionChanged += (sender, args) =>
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

    private bool CanRemoveSelectedField() => CurrentSelectedField != null;

    private void OnRemoveSelectedField()
    {
        CurrentSelectedField!.ContainingCollection = null;
        SelectedFields.Remove(CurrentSelectedField);
    }

    private bool CanAddSelectedField() => CurrentAvailableField != null;

    private void OnAddSelectedField()
    {
        var field = CurrentAvailableField!.MakeCopy();
        field.ContainingCollection = SelectedFields;
        SelectedFields.Add(field);
    }

    private bool CanSelectFields() => SelectedFields.Count > 0;

    private void OnSelectFields()
    {
        var p = new DialogParameters
        {
            { "Fields", SelectedFields }
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

    private FieldViewModel? _currentAvailableField;
    public FieldViewModel? CurrentAvailableField
    {
        get => _currentAvailableField;
        set => SetProperty(ref _currentAvailableField, value);
    }

    private FieldViewModel? _currentSelectedField;
    public FieldViewModel? CurrentSelectedField
    {
        get => _currentSelectedField;
        set => SetProperty(ref _currentSelectedField, value);
    }

    public DelegateCommand SelectFieldsCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AddSelectedFieldCommand { get; }
    public DelegateCommand RemoveSelectedFieldCommand { get; }
    public DelegateCommand ClearSelectedFieldsCommand { get; }
}
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Commands;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class FieldViewModel : BindableBaseWithErrorNotification
{
    internal Field Field { get; }

    [DebuggerStepThrough]
    protected FieldViewModel(Field field)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));


        DeleteCommand = new DelegateCommand(OnDelete);
        CopyToClipboardCommand = new DelegateCommand(OnCopyToClipboard);

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationErrors[nameof(Name)] = "Name can't be empty";
        }
        else
        {
            ValidationErrors.Remove(nameof(Name));
        }

        RaiseErrorsChanged(nameof(Name));
    }

    protected abstract void OnCopyToClipboard();

    private ObservableCollection<FieldViewModel>? _containingCollection;
    public ObservableCollection<FieldViewModel>? ContainingCollection
    {
        get => _containingCollection;
        set => SetProperty(ref _containingCollection, value);
    }

    private void OnDelete()
    {
        Deleted?.Invoke(this, this);
    }

    public string Name
    {
        get => Field.Name;
        set
        {
            if (Field.Name != value)
            {
                Field.Name = value;
                RaisePropertyChanged();
                Validate();
            }
        }
    }

    public event EventHandler<FieldViewModel>? Deleted;

    public abstract FieldViewModel MakeCopy();

    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand CopyToClipboardCommand { get; }
}

public sealed class TextFieldViewModel : FieldViewModel
{
    private readonly TextField _field;

    [DebuggerStepThrough]
    public TextFieldViewModel(TextField? field = null)
        : base(field ?? new TextField())
    {
        _field = (TextField) Field;
    }

    public string Text
    {
        get => _field.Text;
        set
        {
            if (_field.Text != value)
            {
                _field.Text = value;
                RaisePropertyChanged();
            }
        }
    }

    protected override void OnCopyToClipboard()
    {
        Clipboard.SetText(Text ?? string.Empty);
    }

    public override FieldViewModel MakeCopy()
    {
        return new TextFieldViewModel(new TextField { Name = Name });
    }
}

public sealed class PasswordFieldViewModel : FieldViewModel
{
    private readonly PasswordField _field;

    [DebuggerStepThrough]
    public PasswordFieldViewModel(PasswordField? field = null)
        : base(field ?? new PasswordField())
    {
        _field = (PasswordField) Field;
    }

    public string Password
    {
        get => _field.Password;
        set
        {
            if (_field.Password != value)
            {
                _field.Password = value;
                RaisePropertyChanged();
            }
        }
    }

    protected override void OnCopyToClipboard()
    {
        Clipboard.SetText(Password ?? string.Empty);
    }

    public override FieldViewModel MakeCopy()
    {
        return new PasswordFieldViewModel(new PasswordField { Name = Name });
    }
}

public sealed class FieldViewModelConstructor : IFieldVisitor<FieldViewModel>
{
    public FieldViewModel Create(Field field)
    {
        return field.Visit(this);
    }

    FieldViewModel IFieldVisitor<FieldViewModel>.Visit(TextField textField)
        => new TextFieldViewModel(textField);

    FieldViewModel IFieldVisitor<FieldViewModel>.Visit(PasswordField passwordField)
        => new PasswordFieldViewModel(passwordField);
}
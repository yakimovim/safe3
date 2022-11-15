using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain.Model;

namespace EdlinSoftware.Safe.ViewModels;

public abstract partial class FieldViewModel : ObservableViewModelBase
{
    internal Field Field { get; }

    [DebuggerStepThrough]
    protected FieldViewModel(Field field)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));

        ValidateAllProperties();
    }

    [RelayCommand]
    protected abstract void CopyToClipboard();

    [ObservableProperty]
    private ObservableCollection<FieldViewModel>? _containingCollection;

    [RelayCommand]
    private void Delete()
    {
        Deleted?.Invoke(this, this);
    }

    [IsNotNullOrWhiteSpace]
    public string Name
    {
        get => Field.Name;
        set
        {
            if (Field.Name != value)
            {
                OnPropertyChanging();
                Field.Name = value;
                ValidateProperty(value);
                OnPropertyChanged();
            }
        }
    }

    public event EventHandler<FieldViewModel>? Deleted;

    public abstract FieldViewModel MakeCopy();
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
                OnPropertyChanging();
                _field.Text = value;
                OnPropertyChanged();
            }
        }
    }

    protected override void CopyToClipboard()
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
                OnPropertyChanging();
                _field.Password = value;
                OnPropertyChanged();
            }
        }
    }

    protected override void CopyToClipboard()
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
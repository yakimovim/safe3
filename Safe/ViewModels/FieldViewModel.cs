using System;
using System.Diagnostics;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Mvvm;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class FieldViewModel : BindableBase
{
    internal Field Field { get; }

    [DebuggerStepThrough]
    protected FieldViewModel(Field field)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
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
            }
        }
    }
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
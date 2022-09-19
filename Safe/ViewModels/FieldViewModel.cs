using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class FieldViewModel : BindableBase
{
    internal Field Field { get; }

    [DebuggerStepThrough]
    protected FieldViewModel(Field field)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
        DeleteCommand = new DelegateCommand(OnDelete);
        MoveUpCommand = new DelegateCommand(OnMoveUp, CanMoveUp)
            .ObservesProperty(() => ContainingCollection);
        MoveDownCommand = new DelegateCommand(OnMoveDown, CanMoveDown)
            .ObservesProperty(() => ContainingCollection);
    }

    private bool CanMoveDown()
    {
        if (ContainingCollection == null) return false;

        var index = ContainingCollection.IndexOf(this);

        return index < ContainingCollection.Count - 1;
    }

    private void OnMoveDown()
    {
        var index = ContainingCollection!.IndexOf(this);

        ContainingCollection.Move(index, index + 1);

        MoveUpCommand.RaiseCanExecuteChanged();
        MoveDownCommand.RaiseCanExecuteChanged();

        var anotherMovedItem = ContainingCollection[index];

        anotherMovedItem.MoveUpCommand.RaiseCanExecuteChanged();
        anotherMovedItem.MoveDownCommand.RaiseCanExecuteChanged();
    }

    private bool CanMoveUp()
    {
        if (ContainingCollection == null) return false;

        var index = ContainingCollection.IndexOf(this);

        return index > 0;
    }

    private void OnMoveUp()
    {
        var index = ContainingCollection!.IndexOf(this);

        ContainingCollection.Move(index, index - 1);

        MoveUpCommand.RaiseCanExecuteChanged();
        MoveDownCommand.RaiseCanExecuteChanged();

        var anotherMovedItem = ContainingCollection[index];

        anotherMovedItem.MoveUpCommand.RaiseCanExecuteChanged();
        anotherMovedItem.MoveDownCommand.RaiseCanExecuteChanged();
    }

    private ObservableCollection<FieldViewModel>? _containingCollection;
    public ObservableCollection<FieldViewModel>? ContainingCollection
    {
        get => _containingCollection;
        set
        {
            if (_containingCollection != null)
            {
                _containingCollection.CollectionChanged -= OnContainingCollectionChanged;
            }

            SetProperty(ref _containingCollection, value);

            if (_containingCollection != null)
            {
                _containingCollection.CollectionChanged += OnContainingCollectionChanged;
            }
        }
    }

    private void OnContainingCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MoveDownCommand.RaiseCanExecuteChanged();
        MoveUpCommand.RaiseCanExecuteChanged();
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
            }
        }
    }

    public event EventHandler<FieldViewModel> Deleted;

    public abstract FieldViewModel MakeCopy();

    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand MoveUpCommand { get; }
    public DelegateCommand MoveDownCommand { get; }
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
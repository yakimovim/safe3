using System.Collections.ObjectModel;
using System.Windows.Media;
using EdlinSoftware.Safe.Images;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class ItemViewModelBase : ViewModelBase
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value, Validate);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _tags = string.Empty;

    public string Tags
    {
        get => _tags;
        set => SetProperty(ref _tags, value);
    }

    private ImageSource _icon = Icons.DefaultItemIcon;

    public ImageSource Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public ObservableCollection<FieldViewModel> Fields { get; } = new();

    protected virtual void Validate()
    {
        CheckNullOrWhiteSpace(Title, nameof(Title));
    }
}
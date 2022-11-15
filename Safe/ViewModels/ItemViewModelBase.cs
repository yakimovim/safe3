using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using EdlinSoftware.Safe.Images;

namespace EdlinSoftware.Safe.ViewModels;

[ObservableRecipient]
public abstract partial class ItemViewModelBase : ObservableViewModelBase
{
    [ObservableProperty]
    [IsNotNullOrWhiteSpace]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedRecipients]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TagsCollection))]
    private string _tags = string.Empty;

    public IReadOnlyCollection<string> TagsCollection
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Tags)) return Array.Empty<string>();

            return Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    [ObservableProperty]
    private ImageSource _icon = Icons.DefaultItemIcon;

    public ObservableCollection<FieldViewModel> Fields { get; } = new();
}
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Storage.Model;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class EditStorageDetailsViewModel : ObservableViewModelBase
{
    private readonly IStorageInfoRepository _storageInfoRepository;

    public EditStorageDetailsViewModel(
        IStorageInfoRepository storageInfoRepository
        )
    {
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<IconRemoved>()
            .Subscribe(OnIconRemoved);
    }

    private void OnIconRemoved(string iconId)
    {
        if (IconId == iconId)
        {
            IconId = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        _storageInfoRepository.SaveStorageInfo(new StorageInfo
        {
            Title = Title,
            Description = Description,
            IconId = IconId
        });

        RegionManager.RequestNavigationToDetails("StorageDetails");

        EventAggregator.GetEvent<StorageDetailsChanged>().Publish();
    }

    private bool CanSaveChanges() => !string.IsNullOrWhiteSpace(Title);

    [RelayCommand]
    private void Cancel()
    {
        RegionManager.RequestNavigationToDetails("StorageDetails");
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [IsNotNullOrWhiteSpace]
    [NotifyDataErrorInfo]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string? _iconId;

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var storageInfo = _storageInfoRepository.GetStorageInfo();

        Title = storageInfo.Title;
        Description = storageInfo.Description ?? string.Empty;
        IconId = storageInfo.IconId;
    }
}
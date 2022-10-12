using System;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Storage.Model;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class EditStorageDetailsViewModel : ViewModelBase
{
    private readonly IStorageInfoRepository _storageInfoRepository;

    public EditStorageDetailsViewModel(
        IStorageInfoRepository storageInfoRepository
        )
    {
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));

        SaveChangesCommand = new DelegateCommand(OnSaveChanges, CanSaveChanges)
            .ObservesProperty(() => Title);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    private void OnSaveChanges()
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

    private void OnCancel()
    {
        RegionManager.RequestNavigationToDetails("StorageDetails");
    }

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

    private string? _iconId;
    public string? IconId
    {
        get => _iconId;
        set => SetProperty(ref _iconId, value);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var storageInfo = _storageInfoRepository.GetStorageInfo();

        Title = storageInfo.Title;
        Description = storageInfo.Description ?? string.Empty;
        IconId = storageInfo.IconId;
    }

    public DelegateCommand SaveChangesCommand { get; }

    public DelegateCommand CancelCommand { get; }

    private void Validate()
    {
        CheckNullOrWhiteSpace(Title, nameof(Title));
    }
}
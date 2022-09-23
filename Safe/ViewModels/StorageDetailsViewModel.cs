using System;
using EdlinSoftware.Safe.Domain;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class StorageDetailsViewModel : ViewModelBase
{
    private readonly IStorageInfoRepository _storageInfoRepository;

    public StorageDetailsViewModel(IStorageInfoRepository storageInfoRepository)
    {
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var storageInfo = _storageInfoRepository.GetStorageInfo();

        Title = storageInfo.Title;
        Description = storageInfo.Description ?? string.Empty;
    }
}
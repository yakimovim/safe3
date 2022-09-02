using System;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class EditItemViewModel : ViewModelBase
    {
        private readonly IItemsRepository _itemsRepository;

        private Item _item;

        public EditItemViewModel(IItemsRepository itemsRepository)
        {
            _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));

            SaveChangesCommand = new DelegateCommand(OnSaveChanges, CanSaveChanges)
                .ObservesProperty(() => Title);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnSaveChanges()
        {
            _item.Title = Title;
            _item.Description = Description;

            _itemsRepository.SaveItem(_item);

            var parameters = new NavigationParameters { { "Item", _item } };
            RegionManager.RequestNavigate("DetailsRegion", "ItemDetails", parameters);

            EventAggregator.GetEvent<ItemChanged>().Publish(_item);
        }

        private bool CanSaveChanges() => !string.IsNullOrWhiteSpace(Title);

        private void OnCancel()
        {
            var parameters = new NavigationParameters { { "Item", _item } };
            RegionManager.RequestNavigate("DetailsRegion", "ItemDetails", parameters);
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _item = navigationContext.Parameters.GetValue<Item>("Item");

            Title = _item.Title;
            Description = _item.Description;
        }

        public DelegateCommand SaveChangesCommand { get; }

        public DelegateCommand CancelCommand { get; }
    }
}

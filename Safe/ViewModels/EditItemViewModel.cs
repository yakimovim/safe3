using System;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.ViewModels.Helpers;
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
            AddTextFieldCommand = new DelegateCommand(OnAddTextField);
            AddPasswordFieldCommand = new DelegateCommand(OnAddPasswordField);
        }

        private void OnSaveChanges()
        {
            _item.Title = Title;
            _item.Description = Description;

            _item.Fields.Clear();
            _item.Fields.AddRange(Fields.Select(f => f.Field));

            _itemsRepository.SaveItem(_item);

            var parameters = new NavigationParameters { { "Item", _item } };
            RegionManager.RequestNavigationToDetails("ItemDetails", parameters);

            EventAggregator.GetEvent<ItemChanged>().Publish(_item);
        }

        private bool CanSaveChanges() => !string.IsNullOrWhiteSpace(Title);

        private void OnCancel()
        {
            var parameters = new NavigationParameters { { "Item", _item } };
            RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
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

        public ObservableCollection<FieldViewModel> Fields { get; } = new ObservableCollection<FieldViewModel>();

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _item = navigationContext.Parameters.GetValue<Item>("Item");

            Title = _item.Title;
            Description = _item.Description;

            Fields.Clear();

            var fieldConstructor = new FieldViewModelConstructor();
            Fields.AddRange(_item.Fields.Select(f =>
            {
                var field = fieldConstructor.Create(f);
                field.Deleted += OnFieldDeleted;
                return field;
            }));
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            foreach (var field in Fields)
            {
                field.Deleted -= OnFieldDeleted;
            }
        }

        private void OnAddTextField()
        {
            var field = new TextFieldViewModel();
            field.Deleted += OnFieldDeleted;
            Fields.Add(field);
        }

        private void OnAddPasswordField()
        {
            var field = new PasswordFieldViewModel();
            field.Deleted += OnFieldDeleted;
            Fields.Add(field);
        }

        private void OnFieldDeleted(object? sender, FieldViewModel field)
        {
            field.Deleted -= OnFieldDeleted;
            Fields.Remove(field);
        }

        public DelegateCommand SaveChangesCommand { get; }

        public DelegateCommand CancelCommand { get; }

        public DelegateCommand AddTextFieldCommand { get; }

        public DelegateCommand AddPasswordFieldCommand { get; }
    }
}

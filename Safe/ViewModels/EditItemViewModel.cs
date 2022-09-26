using System;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class EditItemViewModel : ItemViewModelBase
    {
        private readonly IItemsRepository _itemsRepository;
        private readonly IIconsRepository _iconsRepository;

        private Item _item;
        private string? _iconId;

        public EditItemViewModel(
            IItemsRepository itemsRepository,
            IIconsRepository iconsRepository)
        {
            _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
            _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

            SaveChangesCommand = new DelegateCommand(OnSaveChanges, CanSaveChanges)
                .ObservesProperty(() => Title)
                .ObservesProperty(() => Fields);
            CancelCommand = new DelegateCommand(OnCancel);
            AddFieldsCommand = new DelegateCommand(OnAddFields);
            ClearIconCommand = new DelegateCommand(OnClearIcon);
            SelectIconCommand = new DelegateCommand(OnSelectIcon);
        }

        private void OnAddFields()
        {
            DialogService.ShowAddFieldsDialog(Fields, OnFieldDeleted);
        }

        private void OnSelectIcon()
        {
            DialogService.ShowDialog("IconsDialog", new DialogParameters(), result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var iconId = result.Parameters.GetValue<string>("IconId");

                    _iconId = iconId;

                    Icon = _iconsRepository.GetIcon(_iconId);
                }
            });
        }

        private void OnClearIcon()
        {
            _iconId = null;
            Icon = Icons.DefaultItemIcon;
        }

        private void OnSaveChanges()
        {
            _item.Title = Title;
            _item.Description = Description;

            _item.Fields.Clear();
            _item.Fields.AddRange(Fields.Select(f => f.Field));

            _item.Tags.Clear();
            if (!string.IsNullOrEmpty(Tags))
            {
                _item.Tags.AddRange(Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));
            }

            _item.IconId = _iconId;

            _itemsRepository.SaveItem(_item);

            var parameters = new NavigationParameters { { "Item", _item } };
            RegionManager.RequestNavigationToDetails("ItemDetails", parameters);

            EventAggregator.GetEvent<ItemChanged>().Publish(_item);
        }

        private bool CanSaveChanges()
        {
            if (Fields.Count > 0 && Fields.Any(f => string.IsNullOrWhiteSpace(f.Name)))
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(Title);
        }

        private void OnCancel()
        {
            var parameters = new NavigationParameters { { "Item", _item } };
            RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _item = navigationContext.Parameters.GetValue<Item>("Item");

            Title = _item.Title;
            Description = _item.Description ?? string.Empty;
            Tags = string.Join(", ", _item.Tags);
            Icon = _iconsRepository.GetIcon(_item.IconId);
            _iconId = _item.IconId;

            Fields.Clear();

            var fieldConstructor = new FieldViewModelConstructor();
            Fields.AddRange(_item.Fields.Select(f =>
            {
                var field = fieldConstructor.Create(f);
                field.ContainingCollection = Fields;
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

        private void OnFieldDeleted(object? sender, FieldViewModel field)
        {
            field.Deleted -= OnFieldDeleted;
            Fields.Remove(field);
        }

        public DelegateCommand SaveChangesCommand { get; }

        public DelegateCommand CancelCommand { get; }

        public DelegateCommand AddFieldsCommand { get; }

        public DelegateCommand ClearIconCommand { get; }

        public DelegateCommand SelectIconCommand { get; }
    }
}

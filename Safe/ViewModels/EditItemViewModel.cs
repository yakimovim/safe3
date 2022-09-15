using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class EditItemViewModel : ViewModelBase
    {
        private readonly IItemsRepository _itemsRepository;
        private readonly IIconsRepository _iconsRepository;

        private Item _item;

        public EditItemViewModel(
            IItemsRepository itemsRepository,
            IIconsRepository iconsRepository)
        {
            _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
            _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

            SaveChangesCommand = new DelegateCommand(OnSaveChanges, CanSaveChanges)
                .ObservesProperty(() => Title);
            CancelCommand = new DelegateCommand(OnCancel);
            AddTextFieldCommand = new DelegateCommand(OnAddTextField);
            AddPasswordFieldCommand = new DelegateCommand(OnAddPasswordField);
            ClearIconCommand = new DelegateCommand(OnClearIcon);
            SelectIconCommand = new DelegateCommand(OnSelectIcon);
        }

        private void OnSelectIcon()
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openDialog.ShowDialog() == true)
            {
                var fileStream = openDialog.OpenFile();

                _iconId = _iconsRepository.CreateNewIcon(fileStream);

                Icon = _iconsRepository.GetIcon(_iconId);
            }
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

        private string _tags;

        public string Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }

        private string? _iconId;
        private ImageSource _icon;
        public ImageSource Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        public ObservableCollection<FieldViewModel> Fields { get; } = new ObservableCollection<FieldViewModel>();

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _item = navigationContext.Parameters.GetValue<Item>("Item");

            Title = _item.Title;
            Description = _item.Description;
            Tags = string.Join(", ", _item.Tags);
            Icon = _iconsRepository.GetIcon(_item.IconId);
            _iconId = _item.IconId;

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

        public DelegateCommand ClearIconCommand { get; }

        public DelegateCommand SelectIconCommand { get; }
    }
}

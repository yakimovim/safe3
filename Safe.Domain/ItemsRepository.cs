using System;
using System.Collections.Generic;
using System.Linq;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Search;
using EdlinSoftware.Safe.Storage;
using IStorageItemsRepository = EdlinSoftware.Safe.Storage.IItemsRepository;

namespace EdlinSoftware.Safe.Domain
{
    public class ItemsRepository : IItemsRepository
    {
        private sealed class StorageFieldConverter : Storage.Model.IFieldVisitor<Field>
        {
            public Field CreateFrom(Storage.Model.Field field)
            {
                var result = field.Visit(this);
                result.Name = field.Name;
                return result;
            }

            Field Storage.Model.IFieldVisitor<Field>.Visit(Storage.Model.TextField textField)
            {
                return new TextField
                {
                    Text = textField.Text
                };
            }

            Field Storage.Model.IFieldVisitor<Field>.Visit(Storage.Model.PasswordField passwordField)
            {
                return new PasswordField
                {
                    Password = passwordField.Password
                };
            }
        }

        private sealed class DomainFieldConverter : IFieldVisitor<Storage.Model.Field>
        {
            public Storage.Model.Field CreateFrom(Field field)
            {
                var result = field.Visit(this);
                result.Name = field.Name;
                return result;
            }

            Storage.Model.Field IFieldVisitor<Storage.Model.Field>.Visit(TextField textField)
            {
                return new Storage.Model.TextField
                {
                    Text = textField.Text
                };
            }

            Storage.Model.Field IFieldVisitor<Storage.Model.Field>.Visit(PasswordField passwordField)
            {
                return new Storage.Model.PasswordField
                {
                    Password = passwordField.Password
                };
            }
        }

        private static readonly StorageFieldConverter _storageFieldConverter = new StorageFieldConverter();
        private static readonly DomainFieldConverter _domainFieldConverter = new DomainFieldConverter();

        private readonly IStorageItemsRepository _itemsRepository;

        public ItemsRepository(
            IStorageItemsRepository itemsRepository
            )
        {
            _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        }

        public IReadOnlyCollection<Item> Find(IReadOnlyCollection<SearchStringElement> searchDefinition)
        {
            var itemsData = _itemsRepository.Find(searchDefinition);

            return ConstructItems(itemsData);
        }

        private IReadOnlyCollection<Item> ConstructItems(IReadOnlyCollection<Storage.Model.Item> itemsData)
        {
            return itemsData
                .Select(ConstructItem)
                .ToArray();
        }

        private Item ConstructItem(Storage.Model.Item itemData)
        {
            var item = new Item
            {
                Id = itemData.Id,
                ParentId = itemData.ParentId,
                Title = itemData.Title,
                Description = itemData.Description,
                IconId = itemData.IconId,
                Tags = new List<string>(itemData.Tags)
            };
            item.Fields.AddRange(itemData.Fields.Select(_storageFieldConverter.CreateFrom));
            return item;
        }

        public IReadOnlyCollection<Item> GetChildItems(Item? parentItem)
        {
            var itemsData = _itemsRepository.GetChildItems(parentItem?.Id);

            return ConstructItems(itemsData);
        }

        public void SaveItem(Item item)
        {
            var itemData = new Storage.Model.Item
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Title = item.Title,
                Description = item.Description,
                IconId = item.IconId,
                Tags = new List<string>(item.Tags)
            };
            itemData.Fields.AddRange(item.Fields.Select(_domainFieldConverter.CreateFrom));

            _itemsRepository.SaveItems(itemData);

            item.Id = itemData.Id;
        }

        public void DeleteItem(Item item)
        {
            _itemsRepository.DeleteItems(item.Id);
        }

        public bool IsChildOrSelfOf(Item item, Item? probableParentItem)
        {
            if (probableParentItem == null) return true;

            if (probableParentItem.Id == item.Id) return true;

            while(item != null)
            {
                if (item.ParentId == probableParentItem.Id) return true;

                if (item.ParentId == null) return false;

                item = ConstructItem(_itemsRepository.GetItem(item.ParentId.Value)!);
            }

            return false;
        }
    }
}
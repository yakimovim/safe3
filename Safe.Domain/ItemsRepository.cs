using System;
using System.Collections.Generic;
using System.Linq;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Storage;
using IStorageItemsRepository = EdlinSoftware.Safe.Storage.IItemsRepository;
using IStorageFieldsRepository = EdlinSoftware.Safe.Storage.IFieldsRepository;

namespace EdlinSoftware.Safe.Domain
{
    public class ItemsRepository : IItemsRepository
    {
        private sealed class StorageFieldConverter : Storage.Model.IFieldVisitor<Field>
        {
            public Field CreateFrom(Storage.Model.Field field)
            {
                var result = field.Visit(this);
                result.Id = field.Id;
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
            private readonly int _itemId;

            public DomainFieldConverter(int itemId)
            {
                _itemId = itemId;
            }

            public Storage.Model.Field CreateFrom(Field field)
            {
                var result = field.Visit(this);
                result.Id = field.Id;
                result.ItemId = _itemId;
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

        private readonly IStorageItemsRepository _itemsRepository;
        private readonly IStorageFieldsRepository _fieldsRepository;

        public ItemsRepository(
            IStorageItemsRepository itemsRepository,
            IStorageFieldsRepository fieldsRepository)
        {
            _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
            _fieldsRepository = fieldsRepository ?? throw new ArgumentNullException(nameof(fieldsRepository));
        }

        public IReadOnlyList<Item> GetChildItems(Item? parentItem)
        {
            var parentId = parentItem?.Id;

            var itemsData = _itemsRepository.GetChildItems(parentId);

            return itemsData
                .Select(i =>
                {
                    var item = new Item
                    {
                        Id = i.Id,
                        ParentId = i.ParentId,
                        Title = i.Title,
                        Description = i.Description,
                        Tags = new List<string>(i.Tags)
                    };
                    FillFields(item);
                    return item;
                })
                .ToArray();
        }

        private void FillFields(Item item)
        {
            var fieldsData = _fieldsRepository.GetItemFields(item.Id);

            var converter = new StorageFieldConverter();

            item.Fields.Clear();

            item.Fields.AddRange(fieldsData.Select(converter.CreateFrom));
        }

        public void SaveItem(Item item)
        {
            var itemData = new Storage.Model.Item
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Title = item.Title,
                Description = item.Description,
                Tags = new List<string>(item.Tags)
            };

            _itemsRepository.SaveItems(itemData);

            item.Id = itemData.Id;

            var oldFieldsData = _fieldsRepository
                .GetItemFields(item.Id)
                .ToDictionary(f => f.Id, f => f);

            var converter = new DomainFieldConverter(item.Id);

            var newFieldsData = item
                .Fields
                .Select(f => new
                {
                    DomainField = f,
                    StorageField = converter.CreateFrom(f)
                })
                .ToArray();

            var order = 0;
            foreach (var fieldsPair in newFieldsData)
            {
                fieldsPair.StorageField.Order = order++;
                if (oldFieldsData.ContainsKey(fieldsPair.StorageField.Id))
                {
                    oldFieldsData.Remove(fieldsPair.StorageField.Id);
                }
            }

            _fieldsRepository.DeleteFields(oldFieldsData.Keys);

            _fieldsRepository.SaveFields(newFieldsData.Select(p => p.StorageField).ToArray());

            foreach (var fieldsPair in newFieldsData)
            {
                fieldsPair.DomainField.Id = fieldsPair.StorageField.Id;
            }
        }

        public void DeleteItem(Item item)
        {
            _itemsRepository.DeleteItems(item.Id);
        }
    }
}
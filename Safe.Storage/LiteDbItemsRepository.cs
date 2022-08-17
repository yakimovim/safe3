using System;
using System.Collections.Generic;
using EdlinSoftware.Safe.Search;
using EdlinSoftware.Safe.Storage.Model;
using LiteDB;

namespace EdlinSoftware.Safe.Storage
{
    public class LiteDbItemsRepository : IItemsRepository
    {
        private readonly ILiteDbConnectionProvider _dbProvider;

        public LiteDbItemsRepository(ILiteDbConnectionProvider dbProvider)
        {
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        private ILiteCollection<Item> Collection => _dbProvider.GetDatabase().GetCollection<Item>();

        public IReadOnlyCollection<Item> Find(IReadOnlyCollection<SearchStringElement> searchDefinition)
        {
            var query = Collection.Query();

            foreach (var searchElement in searchDefinition)
            {
                var searchPattern = $"%{searchElement.Text}%";

                switch (searchElement.Field)
                {
                    case Fields.Title:
                        query = query
                            .Where("$.Title LIKE @0", searchPattern);
                        break;
                    case Fields.Description:
                        query = query
                            .Where("$.Description LIKE @0", searchPattern);
                        break;
                    case Fields.Tag:
                        query = query
                            .Where("$.Tags ANY LIKE @0", searchPattern);
                        break;
                    case Fields.Field:
                        query = query
                            .Where("$.Fields[*].Value ANY LIKE @0", searchPattern);
                        break;
                    case null:
                        query = query
                            .Where("($.Title LIKE @0) OR ($.Description LIKE @0) OR ($.Tags ANY LIKE @0) OR ($.Fields[*].Value ANY LIKE @0)", searchPattern);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return query
                .Limit(20)
                .ToArray();
        }

        public Item? GetItem(int id)
        {
            return Collection.FindById(id);
        }

        public IReadOnlyCollection<Item> GetChildItems(int? parentId)
        {
            return Collection
                .Query()
                .Where(i => i.ParentId == parentId)
                .ToArray();
        }

        public void SaveItems(IReadOnlyCollection<Item> items)
        {
            var collection = Collection;

            var existingItems = new HashSet<int>();

            foreach (var item in items)
            {
                if (item is null) throw new ArgumentException("Unable to save null items", nameof(items));

                var parentId = item.ParentId;

                if(parentId is null) continue;

                if (!existingItems.Contains(parentId.Value))
                {
                    if (collection.Exists(i => i.Id == parentId.Value))
                    {
                        existingItems.Add(parentId.Value);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Parent item {parentId.Value} does not exist");
                    }
                }
            }

            collection.Upsert(items);
        }

        public void DeleteItems(IReadOnlyCollection<int> itemIds)
        {
            var itemsCollection = Collection;

            DeleteItems(itemsCollection, itemIds);
        }

        private void DeleteItems(
            ILiteCollection<Item> itemsCollection,
            IReadOnlyCollection<int> itemIds)
        {
            foreach (var itemId in itemIds)
            {
                var nestedItemIds = itemsCollection
                    .Query()
                    .Where(i => i.ParentId == itemId)
                    .Select(i => i.Id)
                    .ToArray();

                DeleteItems(itemsCollection, nestedItemIds);

                itemsCollection.Delete(itemId);
            }
        }
    }
}
using System;
using System.Collections.Generic;
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

        public Item GetItem(int id)
        {
            return Collection.FindById(id);
        }

        public IReadOnlyList<Item> GetChildItems(int? parentId)
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

                if (existingItems.Contains(parentId.Value))
                    continue;
                else
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
            var fieldsCollection = _dbProvider.GetDatabase().GetCollection<Field>();

            DeleteItems(itemsCollection, fieldsCollection, itemIds);
        }

        private void DeleteItems(
            ILiteCollection<Item> itemsCollection,
            ILiteCollection<Field> fieldsCollection,
            IReadOnlyCollection<int> itemIds)
        {
            foreach (var itemId in itemIds)
            {
                var nestedItemIds = itemsCollection
                    .Query()
                    .Where(i => i.ParentId == itemId)
                    .Select(i => i.Id)
                    .ToArray();

                DeleteItems(itemsCollection, fieldsCollection, nestedItemIds);

                fieldsCollection.DeleteMany(f => f.ItemId == itemId);

                itemsCollection.Delete(itemId);
            }
        }
    }
}
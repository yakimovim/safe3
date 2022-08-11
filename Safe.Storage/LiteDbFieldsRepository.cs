using System;
using System.Collections.Generic;
using EdlinSoftware.Safe.Storage.Model;
using LiteDB;

namespace EdlinSoftware.Safe.Storage
{
    public class LiteDbFieldsRepository : IFieldsRepository
    {
        private readonly ILiteDbConnectionProvider _dbProvider;

        public LiteDbFieldsRepository(ILiteDbConnectionProvider dbProvider)
        {
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        private ILiteCollection<Field> Collection => _dbProvider.GetDatabase().GetCollection<Field>();

        public Field? GetField(int id)
        {
            return Collection.FindById(id);
        }

        public IReadOnlyList<Field> GetItemFields(int itemId)
        {
            return Collection
                .Query()
                .Where(f => f.ItemId == itemId)
                .ToArray();
        }

        public void SaveFields(IReadOnlyCollection<Field> fields)
        {
            var itemsCollection = _dbProvider.GetDatabase().GetCollection<Item>();

            var existingItemIds = new HashSet<int>();
            foreach (var field in fields)
            {
                if (field is null)
                    throw new ArgumentException("Unable to save null field", nameof(fields));

                var itemId = field.ItemId;

                if(existingItemIds.Contains(itemId))
                    continue;
                else
                {
                    var itemExists = itemsCollection.Exists(i => i.Id == itemId);
                    if (itemExists)
                        existingItemIds.Add(itemId);
                    else
                        throw new InvalidOperationException($"Unable to save field for non-existing item {itemId}");
                }
            }

            Collection.Upsert(fields);
        }

        public void DeleteFields(IReadOnlyCollection<int> fieldIds)
        {
            var collection = Collection;

            foreach (var fieldId in fieldIds)
            {
                collection.Delete(fieldId);
            }
        }
    }
}
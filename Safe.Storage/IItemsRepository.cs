using System.Collections.Generic;
using System.Linq;
using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Storage
{
    public interface IItemsRepository
    {
        Item? GetItem(int id);

        IReadOnlyList<Item> GetChildItems(int? parentId);

        void SaveItems(IReadOnlyCollection<Item> items);

        void DeleteItems(IReadOnlyCollection<int> itemIds);
    }

    public static class ItemsRepositoryExtensions
    {
        public static void SaveItems(this IItemsRepository repository, params Item[] items)
        {
            repository.SaveItems(items);
        }

        public static void DeleteItems(this IItemsRepository repository, params int[] itemIds)
        {
            repository.DeleteItems(itemIds);
        }

        public static void DeleteItems(this IItemsRepository repository, params Item[] items)
        {
            repository.DeleteItems(items.Select(i => i.Id).ToArray());
        }

        public static void DeleteItems(this IItemsRepository repository, IReadOnlyCollection<Item> items)
        {
            repository.DeleteItems(items.Select(i => i.Id).ToArray());
        }
    }

}
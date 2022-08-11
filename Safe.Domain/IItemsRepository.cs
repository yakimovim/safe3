using System.Collections.Generic;
using EdlinSoftware.Safe.Domain.Model;

namespace EdlinSoftware.Safe.Domain
{
    public interface IItemsRepository
    {
        public IReadOnlyList<Item> GetChildItems(Item? parentItem);

        public void SaveItem(Item item);

        public void DeleteItem(Item item);
    }
}
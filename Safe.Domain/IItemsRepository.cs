using System.Collections.Generic;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Search;

namespace EdlinSoftware.Safe.Domain
{
    public interface IItemsRepository
    {
        public IReadOnlyCollection<Item> Find(IReadOnlyCollection<SearchStringElement> searchDefinition);

        public IReadOnlyCollection<Item> GetChildItems(Item? parentItem);

        public void SaveItem(Item item);

        public void DeleteItem(Item item);

        public bool IsChildOrSelfOf(Item item, Item? probableParentItem);
    }
}
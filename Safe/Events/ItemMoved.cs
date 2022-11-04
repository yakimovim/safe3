using EdlinSoftware.Safe.Domain.Model;
using Prism.Events;

namespace EdlinSoftware.Safe.Events
{
    internal class ItemMoved : PubSubEvent<(Item MovingItem, Item? TargetItem)>
    {}
}

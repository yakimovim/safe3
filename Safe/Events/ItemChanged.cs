using EdlinSoftware.Safe.Domain.Model;
using Prism.Events;

namespace EdlinSoftware.Safe.Events
{
    internal class ItemChanged : PubSubEvent<Item>
    {
    }
}

using EdlinSoftware.Safe.Domain.Model;
using Prism.Events;

namespace EdlinSoftware.Safe.Events;

public class NewItemCreated : PubSubEvent<(Item newItem, Item? parentItem)>
{
    
}
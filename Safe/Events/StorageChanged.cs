using EdlinSoftware.Safe.Storage;
using Prism.Events;

namespace EdlinSoftware.Safe.Events
{
    internal class StorageChanged : PubSubEvent<ILiteDbConnectionProvider>
    {
    }
}

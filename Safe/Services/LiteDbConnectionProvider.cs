using EdlinSoftware.Safe.Storage;
using LiteDB;

namespace EdlinSoftware.Safe.Services
{
    internal class LiteDbConnectionProvider : ILiteDbConnectionProvider
    {
        public ILiteDatabase Database { get; set; }

        public ILiteDatabase GetDatabase() => Database;
    }
}

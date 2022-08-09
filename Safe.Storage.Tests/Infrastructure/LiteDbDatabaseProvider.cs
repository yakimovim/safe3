using LiteDB;

namespace EdlinSoftware.Safe.Storage.Tests.Infrastructure;

public class LiteDbDatabaseProvider : IDisposable
{
    public LiteDbDatabaseProvider()
    {
        Database = new LiteDatabase("Filename=:memory:");
    }

    public ILiteDatabase Database { get; }

    public void Dispose()
    {
        Database.Dispose();
    }
}
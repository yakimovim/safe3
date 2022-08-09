using LiteDB;

namespace EdlinSoftware.Safe.Storage.Tests.Infrastructure;

public class LiteDbConnectionProvider : ILiteDbConnectionProvider
{
    private readonly ILiteDatabase _database;

    public LiteDbConnectionProvider(ILiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public ILiteDatabase GetDatabase() => _database;
}
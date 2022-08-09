using LiteDB;

namespace EdlinSoftware.Safe.Storage
{
    public interface ILiteDbConnectionProvider
    {
        ILiteDatabase GetDatabase();
    }
}
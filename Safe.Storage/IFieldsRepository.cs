using System.Collections.Generic;
using System.Linq;
using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Storage
{
    public interface IFieldsRepository
    {
        Field GetField(int id);

        IReadOnlyList<Field> GetItemFields(int itemId);

        void SaveFields(IReadOnlyCollection<Field> fields);

        void DeleteFields(IReadOnlyCollection<int> fieldIds);
    }

    public static class FieldsRepositoryExtensions
    {
        public static void SaveFields(this IFieldsRepository repository, params Field[] fields)
        {
            repository.SaveFields(fields);
        }

        public static void DeleteFields(this IFieldsRepository repository, params Field[] fields)
        {
            repository.DeleteFields(fields.Select(f => f.Id).ToArray());
        }

        public static void DeleteFields(this IFieldsRepository repository, IReadOnlyCollection<Field> fields)
        {
            repository.DeleteFields(fields.Select(f => f.Id).ToArray());
        }

        public static void DeleteFields(this IFieldsRepository repository, params int[] fieldIds)
        {
            repository.DeleteFields(fieldIds);
        }
    }

}
using Bogus;
using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Storage.Tests.Infrastructure;

public static class Generators
{
    private static readonly Faker _faker = new Faker();

    public static Item CreateItem(int? parentId = null)
    {
        return new Item
        {
            Title = _faker.Name.JobTitle(),
            Description = _faker.Name.JobDescriptor(),
            ParentId = parentId
        };
    }

    public static TextField CreateTextField(int itemId)
    {
        return new TextField
        {
            ItemId = itemId,
            Name = "URL:",
            Text = _faker.Internet.Url(),
        };
    }

    public static IReadOnlyList<Field> CreateSeveralFields(int count, int itemId)
    {
        return Enumerable
            .Range(0, count)
            .Select(_ => CreateTextField(itemId))
            .Cast<Field>()
            .ToArray();
    }
}
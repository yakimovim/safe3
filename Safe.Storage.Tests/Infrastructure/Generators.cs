using Bogus;
using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Storage.Tests.Infrastructure;

public static class Generators
{
    private static readonly Faker DataFaker = new Faker();

    public static Item CreateItem(int? parentId = null)
    {
        return new Item
        {
            Title = DataFaker.Name.JobTitle(),
            Description = DataFaker.Name.JobDescriptor(),
            ParentId = parentId,
            Tags = new List<string>(DataFaker.Commerce.Categories(2))
        };
    }

    public static TextField CreateTextField()
    {
        return new TextField
        {
            Name = "URL:",
            Text = DataFaker.Internet.Url(),
        };
    }

    public static IReadOnlyList<Field> CreateSeveralFields(int count)
    {
        return Enumerable
            .Range(0, count)
            .Select(_ => CreateTextField())
            .Cast<Field>()
            .ToArray();
    }
}
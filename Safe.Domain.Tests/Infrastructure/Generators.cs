using Bogus;
using EdlinSoftware.Safe.Domain.Model;

namespace EdlinSoftware.Safe.Domain.Tests.Infrastructure;

public static class Generators
{
    private static readonly Faker DataFaker = new Faker();

    public static Item CreateItem(Item? parentItem = null)
    {
        return new Item(parentItem)
        {
            Title = DataFaker.Name.JobTitle(),
            Description = DataFaker.Name.JobDescriptor(),
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
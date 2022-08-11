using Bogus;
using EdlinSoftware.Safe.Domain.Model;

namespace EdlinSoftware.Safe.Domain.Tests.Infrastructure;

public static class Generators
{
    private static readonly Faker _faker = new Faker();

    public static Item CreateItem(Item? parentItem = null)
    {
        return new Item(parentItem)
        {
            Title = _faker.Name.JobTitle(),
            Description = _faker.Name.JobDescriptor(),
            Tags = new List<string>(_faker.Commerce.Categories(2))
        };
    }

    public static TextField CreateTextField()
    {
        return new TextField
        {
            Name = "URL:",
            Text = _faker.Internet.Url(),
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
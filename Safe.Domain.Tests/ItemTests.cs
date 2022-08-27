using EdlinSoftware.Safe.Domain.Model;
using FluentAssertions;

namespace EdlinSoftware.Safe.Domain.Tests;

public class ItemTests
{
    [Fact]
    public void Cant_create_item_for_unsaved_parent()
    {
        Action act = () => new Item(new Item());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, false)]
    public void Compare_items(int id1, int id2, bool equal)
    {
        var item1 = new Item { Id = id1 };
        var item2 = new Item { Id = id2 };

        item1.Should().NotBeSameAs(item2);
        item1.Equals(item2).Should().Be(equal);
    }
}
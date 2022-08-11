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
}
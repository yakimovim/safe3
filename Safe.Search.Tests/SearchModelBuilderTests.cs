namespace EdlinSoftware.Safe.Search.Tests;

public class SearchModelBuilderTests
{
    private readonly SearchModelBuilder _builder;

    public SearchModelBuilderTests()
    {
        _builder = new SearchModelBuilder();
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("Привет")]
    public void SingleWord(string input)
    {
        var elements = _builder.GetSearchStringElements(input);

        elements.Should().NotBeNull();
        elements.Should().HaveCount(1);

        elements.Single().Field.Should().BeNull();
        elements.Single().Text.Should().Be(input);
    }

    [Theory]
    [InlineData("\"Hello, Ivan\"")]
    [InlineData("\"Привет, Ваня\"")]
    public void SeveralWords(string input)
    {
        var elements = _builder.GetSearchStringElements(input);

        elements.Should().NotBeNull();
        elements.Should().HaveCount(1);

        elements.Single().Field.Should().BeNull();
        elements.Single().Text.Should().Be(input.Trim('"'));
    }

    [Theory]
    [InlineData("title:Microsoft", Fields.Title)]
    [InlineData("Description:test", Fields.Description)]
    [InlineData("tag:Shop", Fields.Tag)]
    [InlineData("field:yandex", Fields.Field)]
    public void FieldWithSingleWord(string input, Fields field)
    {
        var parts = input.Split(':');

        var elements = _builder.GetSearchStringElements(input);

        elements.Should().NotBeNull();
        elements.Should().HaveCount(1);

        elements.Single().Field.Should().Be(field);
        elements.Single().Text.Should().Be(parts[1]);
    }

    [Theory]
    [InlineData("http://www.yandex.ru")]
    [InlineData("mailto:shop@contoso.com")]
    public void TextWithColon(string input)
    {
        var elements = _builder.GetSearchStringElements(input);

        elements.Should().NotBeNull();
        elements.Should().HaveCount(1);

        elements.Single().Field.Should().BeNull();
        elements.Single().Text.Should().Be(input);
    }

    [Theory]
    [InlineData("title:\"Microsoft Outlook\"", Fields.Title)]
    [InlineData("Description:\"test case\"", Fields.Description)]
    [InlineData("tag:\"Online Shop\"", Fields.Tag)]
    [InlineData("field:\"yandex browser\"", Fields.Field)]
    [InlineData("field:\"http://www.contoso.com\"", Fields.Field)]
    public void FieldWithSeveralWords(string input, Fields field)
    {
        var parts = input.Split(':');

        var elements = _builder.GetSearchStringElements(input);

        elements.Should().NotBeNull();
        elements.Should().HaveCount(1);

        elements.Single().Field.Should().Be(field);
        elements.Single().Text.Should().Be(string.Join(':', parts.Skip(1)).Trim('"'));
    }

    [Fact]
    public void MultipleParts()
    {
        var elements = _builder.GetSearchStringElements(
            "Shop field:Microsoft \"To do\""
        );

        elements.Should().NotBeNull();
        elements.Should().HaveCount(3);

        var element = elements.ElementAt(0);
        element.Field.Should().BeNull();
        element.Text.Should().Be("Shop");

        element = elements.ElementAt(1);
        element.Field.Should().Be(Fields.Field);
        element.Text.Should().Be("Microsoft");

        element = elements.ElementAt(2);
        element.Field.Should().BeNull();
        element.Text.Should().Be("To do");
    }
}
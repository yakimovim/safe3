using EdlinSoftware.Safe.Services;

namespace EdlinSoftware.Safe.Tests.Services;

public class PasswordGeneratorTests
{
    private readonly PasswordGenerator _generator = new PasswordGenerator();

    [Fact]
    public void Dont_allow_non_positive_length()
    {
        // Arrange

        Action act = () =>
            _generator.Generate(0, true, true, true);

        // Act and assert

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Some_set_of_symbols_should_be_used()
    {
        // Arrange

        Action act = () =>
            _generator.Generate(10, false, false, false);

        // Act and assert

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [AutoData]
    public void Generated_password_should_be_of_requested_length(uint length)
    {
        // Act

        var password = _generator.Generate(length, true, true, true);

        // Assert

        password.Length.Should().Be((int)length);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    public void Generated_password_must_contain_only_requested_symbols(
        bool useLetters,
        bool useDigits,
        bool usePunctuation
    )
    {
        // Act

        var password = _generator.Generate(10, useLetters, useDigits, usePunctuation);

        // Assert

        if(!useLetters)
        {
            password.Should().NotMatchRegex("[a-zA-Z]");
        }
        if (!useDigits)
        {
            password.Should().NotMatchRegex("[0-9]");
        }
        if (!usePunctuation)
        {
            password.Should().NotMatchRegex("[!?@#$%&*]");
        }
    }
}
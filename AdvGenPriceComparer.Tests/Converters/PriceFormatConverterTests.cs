using System;
using FluentAssertions;
using Xunit;
using AdvGenPriceComparer.Desktop.WinUI.Converters;

namespace AdvGenPriceComparer.Tests.Converters;

public class PriceFormatConverterTests
{
    private readonly PriceFormatConverter _converter;

    public PriceFormatConverterTests()
    {
        _converter = new PriceFormatConverter();
    }

    [Theory]
    [InlineData(10.5, "$10.50")]
    [InlineData(0, "$0.00")]
    [InlineData(99.99, "$99.99")]
    [InlineData(1234.567, "$1234.57")]
    public void Convert_WithDouble_ReturnsFormattedCurrency(double value, string expected)
    {
        // Act
        var result = _converter.Convert(value, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("10.5", "$10.50")]
    [InlineData("0", "$0.00")]
    [InlineData("99.99", "$99.99")]
    [InlineData("1234.567", "$1234.57")]
    public void Convert_WithDecimal_ReturnsFormattedCurrency(string decimalString, string expected)
    {
        // Arrange
        decimal value = decimal.Parse(decimalString);

        // Act
        var result = _converter.Convert(value, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsZeroCurrency()
    {
        // Act
        var result = _converter.Convert(null, typeof(string), null, "en-US");

        // Assert
        result.Should().Be("$0.00");
    }

    [Fact]
    public void Convert_WithNonNumericType_ReturnsZeroCurrency()
    {
        // Act
        var result = _converter.Convert("Not a number", typeof(string), null, "en-US");

        // Assert
        result.Should().Be("$0.00");
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        var action = () => _converter.ConvertBack("$10.50", typeof(decimal), null, "en-US");
        action.Should().Throw<NotImplementedException>();
    }
}

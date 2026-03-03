using System;
using FluentAssertions;
using Xunit;
using AdvGenPriceComparer.Desktop.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace AdvGenPriceComparer.WinUI.Tests.Converters;

public class StringToVisibilityConverterTests
{
    private readonly StringToVisibilityConverter _converter;

    public StringToVisibilityConverterTests()
    {
        _converter = new StringToVisibilityConverter();
    }

    [Theory]
    [InlineData("Some text", Visibility.Visible)]
    [InlineData("A", Visibility.Visible)]
    [InlineData(" ", Visibility.Visible)] // Whitespace is not null or empty
    public void Convert_WithNonEmptyString_ReturnsVisible(string input, Visibility expected)
    {
        // Act
        var result = _converter.Convert(input, typeof(Visibility), null!, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", Visibility.Collapsed)]
    [InlineData(null, Visibility.Collapsed)]
    public void Convert_WithNullOrEmptyString_ReturnsCollapsed(string? input, Visibility expected)
    {
        // Act
        var result = _converter.Convert(input!, typeof(Visibility), null!, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_WithNonStringObject_ReturnsCollapsed()
    {
        // Arrange
        var input = new object();

        // Act
        var result = _converter.Convert(input, typeof(Visibility), null!, "en-US");

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(string), null!, "en-US");

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}

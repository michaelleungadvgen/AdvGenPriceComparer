using System;
using AdvGenPriceComparer.Core.ViewModels;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels;

public class DietaryFlagViewModelTests
{
    [Fact]
    public void Constructor_SetsNameCorrectly()
    {
        // Arrange
        var name = "Vegan";

        // Act
        var viewModel = new DietaryFlagViewModel { Name = name };

        // Assert
        Assert.Equal(name, viewModel.Name);
    }

    [Fact]
    public void IsSelected_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var viewModel = new DietaryFlagViewModel { Name = "Test" };

        // Assert
        Assert.False(viewModel.IsSelected);
    }

    [Fact]
    public void IsSelected_SetValue_UpdatesProperty()
    {
        // Arrange
        var viewModel = new DietaryFlagViewModel { Name = "Test" };

        // Act
        viewModel.IsSelected = true;

        // Assert
        Assert.True(viewModel.IsSelected);
    }

    [Fact]
    public void IsSelected_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new DietaryFlagViewModel { Name = "Test" };
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DietaryFlagViewModel.IsSelected))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        viewModel.IsSelected = true;

        // Assert
        Assert.True(propertyChangedRaised);
    }
}

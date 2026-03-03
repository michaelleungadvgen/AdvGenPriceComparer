using System.Collections.Generic;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels.WinUI
{
    public class AllergenViewModelTests
    {
        [Fact]
        public void IsSelected_SetValue_RaisesPropertyChangedEvent()
        {
            // Arrange
            var viewModel = new AllergenViewModel { Name = "Peanuts" };
            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (s, e) => {
                if (e.PropertyName != null)
                {
                    raisedProperties.Add(e.PropertyName);
                }
            };

            // Act
            viewModel.IsSelected = true;

            // Assert
            Assert.Contains(nameof(AllergenViewModel.IsSelected), raisedProperties);
            Assert.True(viewModel.IsSelected);
        }

        [Fact]
        public void IsSelected_SetSameValue_DoesNotRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = new AllergenViewModel { Name = "Peanuts" };
            viewModel.IsSelected = true;

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (s, e) => {
                if (e.PropertyName != null)
                {
                    raisedProperties.Add(e.PropertyName);
                }
            };

            // Act
            viewModel.IsSelected = true;

            // Assert
            Assert.DoesNotContain(nameof(AllergenViewModel.IsSelected), raisedProperties);
            Assert.True(viewModel.IsSelected);
        }

        [Fact]
        public void Constructor_SetName_InitializesCorrectly()
        {
            // Arrange & Act
            var name = "Gluten";
            var viewModel = new AllergenViewModel { Name = name };

            // Assert
            Assert.Equal(name, viewModel.Name);
            Assert.False(viewModel.IsSelected); // Default is false
        }
    }
}
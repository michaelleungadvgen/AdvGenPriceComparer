using System;

namespace AdvGenPriceComparer.Core.ViewModels;

public class DietaryFlagViewModel : CoreViewModelBase
{
    private bool _isSelected;

    public required string Name { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

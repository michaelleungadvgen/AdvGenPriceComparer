using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Desktop.WinUI.Services;

public interface IDialogService
{
    Task<bool> ShowAddItemDialogAsync(ItemViewModel itemViewModel);
    Task<bool> ShowAddPlaceDialogAsync(PlaceViewModel placeViewModel);
    Task ShowComparePricesDialogAsync(IEnumerable<(Item item, decimal lowestPrice, Place place)> bestDeals);
    Task ShowAnalyticsDialogAsync(Dictionary<string, object> stats);
    Task ShowNetworkDialogAsync(NetworkInfo networkInfo);
    Task<bool> ShowConfirmationDialogAsync(string title, string message);
    Task ShowMessageDialogAsync(string title, string message);
}
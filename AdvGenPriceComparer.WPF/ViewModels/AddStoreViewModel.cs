using System;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AddStoreViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;

    private string _storeName = string.Empty;
    private string _chain = string.Empty;
    private string _address = string.Empty;
    private string _suburb = string.Empty;
    private string _state = "QLD";
    private string _postcode = string.Empty;
    private string _phone = string.Empty;

    public AddStoreViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
    }

    public string StoreName
    {
        get => _storeName;
        set => SetProperty(ref _storeName, value);
    }

    public string Chain
    {
        get => _chain;
        set => SetProperty(ref _chain, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string Suburb
    {
        get => _suburb;
        set => SetProperty(ref _suburb, value);
    }

    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public string Postcode
    {
        get => _postcode;
        set => SetProperty(ref _postcode, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public bool SaveStore()
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(StoreName))
        {
            _dialogService.ShowWarning("Store name is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Chain))
        {
            _dialogService.ShowWarning("Chain is required.");
            return false;
        }

        try
        {
            var storeId = _dataService.AddSupermarket(
                StoreName,
                Chain,
                string.IsNullOrWhiteSpace(Address) ? null : Address,
                string.IsNullOrWhiteSpace(Suburb) ? null : Suburb,
                string.IsNullOrWhiteSpace(State) ? null : State,
                string.IsNullOrWhiteSpace(Postcode) ? null : Postcode);

            // Update phone if provided
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                var place = _dataService.GetPlaceById(storeId);
                if (place != null)
                {
                    place.Phone = Phone;
                    _dataService.Places.Update(place);
                }
            }

            _dialogService.ShowSuccess($"Store '{StoreName}' added successfully!");
            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to add store: {ex.Message}");
            return false;
        }
    }
}

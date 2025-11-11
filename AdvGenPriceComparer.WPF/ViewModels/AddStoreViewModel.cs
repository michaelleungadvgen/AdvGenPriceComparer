using System;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AddStoreViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;

    private string? _storeId;
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

    public string? StoreId
    {
        get => _storeId;
        set => SetProperty(ref _storeId, value);
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
            if (string.IsNullOrEmpty(StoreId))
            {
                // Add new store
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
            }
            else
            {
                // Update existing store
                var store = _dataService.GetPlaceById(StoreId);
                if (store != null)
                {
                    store.Name = StoreName;
                    store.Chain = Chain;
                    store.Address = string.IsNullOrWhiteSpace(Address) ? null : Address;
                    store.Suburb = string.IsNullOrWhiteSpace(Suburb) ? null : Suburb;
                    store.State = string.IsNullOrWhiteSpace(State) ? null : State;
                    store.Postcode = string.IsNullOrWhiteSpace(Postcode) ? null : Postcode;
                    store.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone;

                    _dataService.Places.Update(store);
                    _dialogService.ShowSuccess($"Store '{StoreName}' updated successfully!");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to save store: {ex.Message}");
            return false;
        }
    }
}

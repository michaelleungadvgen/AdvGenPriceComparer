using System;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AddStoreViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;

    private string? _storeId;
    private string _storeName = string.Empty;
    private string _chain = string.Empty;
    private string _address = string.Empty;
    private string _suburb = string.Empty;
    private string _state = "QLD";
    private string _postcode = string.Empty;
    private string _phone = string.Empty;

    public AddStoreViewModel(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator;
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
                var result = _mediator.Send(new CreatePlaceCommand(
                    StoreName,
                    Chain,
                    string.IsNullOrWhiteSpace(Address) ? null : Address,
                    string.IsNullOrWhiteSpace(Suburb) ? null : Suburb,
                    string.IsNullOrWhiteSpace(State) ? null : State,
                    string.IsNullOrWhiteSpace(Postcode) ? null : Postcode,
                    string.IsNullOrWhiteSpace(Phone) ? null : Phone
                )).GetAwaiter().GetResult();

                if (!result.Success)
                {
                    _dialogService.ShowError($"Failed to add store: {result.ErrorMessage}");
                    return false;
                }

                _dialogService.ShowSuccess($"Store '{StoreName}' added successfully!");
            }
            else
            {
                // Update existing store
                var result = _mediator.Send(new UpdatePlaceCommand(
                    StoreId,
                    StoreName,
                    Chain,
                    string.IsNullOrWhiteSpace(Address) ? null : Address,
                    string.IsNullOrWhiteSpace(Suburb) ? null : Suburb,
                    string.IsNullOrWhiteSpace(State) ? null : State,
                    string.IsNullOrWhiteSpace(Postcode) ? null : Postcode,
                    string.IsNullOrWhiteSpace(Phone) ? null : Phone
                )).GetAwaiter().GetResult();

                if (!result.Success)
                {
                    _dialogService.ShowError($"Failed to update store: {result.ErrorMessage}");
                    return false;
                }

                _dialogService.ShowSuccess($"Store '{StoreName}' updated successfully!");
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

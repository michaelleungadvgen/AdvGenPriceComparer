using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Desktop.WinUI.ViewModels;

public class PlaceViewModel : BaseViewModel
{
    private string _storeName = string.Empty;
    private string _chain = string.Empty;
    private string _address = string.Empty;
    private string _suburb = string.Empty;
    private string _state = string.Empty;
    private string _postcode = string.Empty;
    private string _phone = string.Empty;
    private string _website = string.Empty;
    private string _operatingHours = string.Empty;
    private string _latitude = string.Empty;
    private string _longitude = string.Empty;
    private bool _isValid = false;
    private string _validationErrors = string.Empty;

    private readonly Regex _phoneRegex = new(@"^\(?\d{2}\)?[\s\-]?\d{4}[\s\-]?\d{4}$");
    private readonly Regex _postcodeRegex = new(@"^\d{4}$");
    private readonly Regex _websiteRegex = new(@"^https?://[^\s]+$");
    private readonly Regex _coordinateRegex = new(@"^-?\d+\.?\d*$");

    public PlaceViewModel()
    {
        InitializeCollections();
        PropertyChanged += (s, e) => ValidatePlace();
    }

    #region Properties

    [Required(ErrorMessage = "Store name is required")]
    public string StoreName
    {
        get => _storeName;
        set => SetProperty(ref _storeName, value);
    }

    [Required(ErrorMessage = "Chain is required")]
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

    [Required(ErrorMessage = "Suburb is required")]
    public string Suburb
    {
        get => _suburb;
        set => SetProperty(ref _suburb, value);
    }

    [Required(ErrorMessage = "State is required")]
    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public string Postcode
    {
        get => _postcode;
        set => SetProperty(ref _postcode, value, OnPostcodeChanged);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value, OnPhoneChanged);
    }

    public string Website
    {
        get => _website;
        set => SetProperty(ref _website, value, OnWebsiteChanged);
    }

    public string OperatingHours
    {
        get => _operatingHours;
        set => SetProperty(ref _operatingHours, value);
    }

    public string Latitude
    {
        get => _latitude;
        set => SetProperty(ref _latitude, value, OnCoordinateChanged);
    }

    public string Longitude
    {
        get => _longitude;
        set => SetProperty(ref _longitude, value, OnCoordinateChanged);
    }

    public bool IsValid
    {
        get => _isValid;
        private set => SetProperty(ref _isValid, value);
    }

    public string ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    // Validation error properties
    private bool _hasPostcodeError;
    public bool HasPostcodeError
    {
        get => _hasPostcodeError;
        private set => SetProperty(ref _hasPostcodeError, value);
    }

    private string _postcodeError = string.Empty;
    public string PostcodeError
    {
        get => _postcodeError;
        private set => SetProperty(ref _postcodeError, value);
    }

    private bool _hasPhoneError;
    public bool HasPhoneError
    {
        get => _hasPhoneError;
        private set => SetProperty(ref _hasPhoneError, value);
    }

    private string _phoneError = string.Empty;
    public string PhoneError
    {
        get => _phoneError;
        private set => SetProperty(ref _phoneError, value);
    }

    private bool _hasWebsiteError;
    public bool HasWebsiteError
    {
        get => _hasWebsiteError;
        private set => SetProperty(ref _hasWebsiteError, value);
    }

    private string _websiteError = string.Empty;
    public string WebsiteError
    {
        get => _websiteError;
        private set => SetProperty(ref _websiteError, value);
    }

    private bool _hasLatitudeError;
    public bool HasLatitudeError
    {
        get => _hasLatitudeError;
        private set => SetProperty(ref _hasLatitudeError, value);
    }

    private string _latitudeError = string.Empty;
    public string LatitudeError
    {
        get => _latitudeError;
        private set => SetProperty(ref _latitudeError, value);
    }

    private bool _hasLongitudeError;
    public bool HasLongitudeError
    {
        get => _hasLongitudeError;
        private set => SetProperty(ref _hasLongitudeError, value);
    }

    private string _longitudeError = string.Empty;
    public string LongitudeError
    {
        get => _longitudeError;
        private set => SetProperty(ref _longitudeError, value);
    }

    #endregion

    #region Collections

    public ObservableCollection<string> Chains { get; } = new();
    public ObservableCollection<string> States { get; } = new();
    public ObservableCollection<ServiceViewModel> Services { get; } = new();

    #endregion

    #region Methods

    private void InitializeCollections()
    {
        var chains = new[] { "Coles", "Woolworths", "IGA", "ALDI", "Foodworks", "Drakes", "Foodland", "Spudshed", "Harris Farm Markets", "Other" };
        foreach (var chain in chains)
        {
            Chains.Add(chain);
        }

        var states = new[] { "NSW", "VIC", "QLD", "WA", "SA", "TAS", "NT", "ACT" };
        foreach (var state in states)
        {
            States.Add(state);
        }

        var services = new[]
        {
            ("24/7", "24_hour"),
            ("Pharmacy", "pharmacy"),
            ("Deli", "deli"),
            ("Bakery", "bakery"),
            ("Butcher", "butcher"),
            ("Seafood", "seafood"),
            ("Self Checkout", "self_checkout"),
            ("Click & Collect", "click_collect"),
            ("Home Delivery", "delivery")
        };

        foreach (var (displayName, tag) in services)
        {
            Services.Add(new ServiceViewModel { DisplayName = displayName, Tag = tag });
        }
    }

    private void OnPostcodeChanged()
    {
        if (string.IsNullOrEmpty(Postcode))
        {
            HasPostcodeError = false;
            PostcodeError = string.Empty;
            return;
        }

        if (!_postcodeRegex.IsMatch(Postcode))
        {
            HasPostcodeError = true;
            PostcodeError = "Postcode must be 4 digits";
        }
        else
        {
            HasPostcodeError = false;
            PostcodeError = string.Empty;
        }
    }

    private void OnPhoneChanged()
    {
        if (string.IsNullOrEmpty(Phone))
        {
            HasPhoneError = false;
            PhoneError = string.Empty;
            return;
        }

        if (!_phoneRegex.IsMatch(Phone))
        {
            HasPhoneError = true;
            PhoneError = "Phone format: (02) 9876 5432 or 02 9876 5432";
        }
        else
        {
            HasPhoneError = false;
            PhoneError = string.Empty;
        }
    }

    private void OnWebsiteChanged()
    {
        if (string.IsNullOrEmpty(Website))
        {
            HasWebsiteError = false;
            WebsiteError = string.Empty;
            return;
        }

        if (!_websiteRegex.IsMatch(Website))
        {
            HasWebsiteError = true;
            WebsiteError = "Website must start with http:// or https://";
        }
        else
        {
            HasWebsiteError = false;
            WebsiteError = string.Empty;
        }
    }

    private void OnCoordinateChanged()
    {
        // Validate latitude
        if (string.IsNullOrEmpty(Latitude))
        {
            HasLatitudeError = false;
            LatitudeError = string.Empty;
        }
        else if (!_coordinateRegex.IsMatch(Latitude))
        {
            HasLatitudeError = true;
            LatitudeError = "Invalid latitude format";
        }
        else if (double.TryParse(Latitude, out var lat) && (lat < -90 || lat > 90))
        {
            HasLatitudeError = true;
            LatitudeError = "Latitude must be between -90 and 90";
        }
        else
        {
            HasLatitudeError = false;
            LatitudeError = string.Empty;
        }

        // Validate longitude
        if (string.IsNullOrEmpty(Longitude))
        {
            HasLongitudeError = false;
            LongitudeError = string.Empty;
        }
        else if (!_coordinateRegex.IsMatch(Longitude))
        {
            HasLongitudeError = true;
            LongitudeError = "Invalid longitude format";
        }
        else if (double.TryParse(Longitude, out var lng) && (lng < -180 || lng > 180))
        {
            HasLongitudeError = true;
            LongitudeError = "Longitude must be between -180 and 180";
        }
        else
        {
            HasLongitudeError = false;
            LongitudeError = string.Empty;
        }
    }

    private void ValidatePlace()
    {
        var validationErrors = new List<string>();

        // Required field validation
        if (string.IsNullOrWhiteSpace(StoreName))
            validationErrors.Add("Store name is required");

        if (string.IsNullOrWhiteSpace(Chain))
            validationErrors.Add("Chain is required");

        if (string.IsNullOrWhiteSpace(Suburb))
            validationErrors.Add("Suburb is required");

        if (string.IsNullOrWhiteSpace(State))
            validationErrors.Add("State is required");

        // Add field-specific validation errors
        if (HasPostcodeError)
            validationErrors.Add(PostcodeError);
        if (HasPhoneError)
            validationErrors.Add(PhoneError);
        if (HasWebsiteError)
            validationErrors.Add(WebsiteError);
        if (HasLatitudeError)
            validationErrors.Add(LatitudeError);
        if (HasLongitudeError)
            validationErrors.Add(LongitudeError);

        IsValid = !validationErrors.Any();
        ValidationErrors = validationErrors.Any() ? string.Join("\n• ", validationErrors) : string.Empty;
    }

    public Place CreatePlace()
    {
        var place = new Place
        {
            Name = StoreName.Trim(),
            Chain = Chain.Trim(),
            Suburb = Suburb.Trim(),
            State = State.Trim()
        };

        if (!string.IsNullOrEmpty(Address?.Trim()))
            place.Address = Address.Trim();

        if (!string.IsNullOrEmpty(Postcode?.Trim()))
            place.Postcode = Postcode.Trim();

        if (!string.IsNullOrEmpty(Phone?.Trim()))
            place.Phone = Phone.Trim();

        if (!string.IsNullOrEmpty(Website?.Trim()))
            place.Website = Website.Trim();

        if (!string.IsNullOrEmpty(OperatingHours?.Trim()))
            place.OperatingHours = OperatingHours.Trim();

        // Services
        var selectedServices = Services.Where(s => s.IsSelected).Select(s => s.Tag).ToList();
        place.Services = selectedServices;

        // GPS Coordinates
        if (double.TryParse(Latitude?.Trim(), out var latitude))
            place.Latitude = latitude;

        if (double.TryParse(Longitude?.Trim(), out var longitude))
            place.Longitude = longitude;

        return place;
    }

    public void ClearForm()
    {
        StoreName = string.Empty;
        Chain = string.Empty;
        Address = string.Empty;
        Suburb = string.Empty;
        State = string.Empty;
        Postcode = string.Empty;
        Phone = string.Empty;
        Website = string.Empty;
        OperatingHours = string.Empty;
        Latitude = string.Empty;
        Longitude = string.Empty;

        foreach (var service in Services)
            service.IsSelected = false;
    }

    #endregion
}

public class ServiceViewModel : BaseViewModel
{
    private bool _isSelected;

    public required string DisplayName { get; init; }
    public required string Tag { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
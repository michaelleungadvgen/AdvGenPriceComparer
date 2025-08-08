using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdvGenPriceComparer.Desktop.WinUI.Controls;

public sealed partial class AddPlaceControl : UserControl
{
    public event EventHandler<bool> ValidationChanged;

    private readonly Regex _phoneRegex = new(@"^\(?\d{2}\)?[\s\-]?\d{4}[\s\-]?\d{4}$");
    private readonly Regex _postcodeRegex = new(@"^\d{4}$");
    private readonly Regex _websiteRegex = new(@"^https?://[^\s]+$");
    private readonly Regex _coordinateRegex = new(@"^-?\d+\.?\d*$");

    public AddPlaceControl()
    {
        this.InitializeComponent();
    }

    private void OnFieldChanged(object sender, object e)
    {
        ValidateForm();
    }

    private void OnPostcodeChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var postcode = textBox.Text.Trim();
            if (!string.IsNullOrEmpty(postcode))
            {
                if (!_postcodeRegex.IsMatch(postcode))
                {
                    PostcodeErrorText.Text = "Postcode must be 4 digits";
                    PostcodeErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    PostcodeErrorText.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PostcodeErrorText.Visibility = Visibility.Collapsed;
            }
        }
        
        ValidateForm();
    }

    private void OnPhoneChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var phone = textBox.Text.Trim();
            if (!string.IsNullOrEmpty(phone))
            {
                if (!_phoneRegex.IsMatch(phone))
                {
                    PhoneErrorText.Text = "Phone format: (02) 9876 5432 or 02 9876 5432";
                    PhoneErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    PhoneErrorText.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PhoneErrorText.Visibility = Visibility.Collapsed;
            }
        }
        
        ValidateForm();
    }

    private void OnWebsiteChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var website = textBox.Text.Trim();
            if (!string.IsNullOrEmpty(website))
            {
                if (!_websiteRegex.IsMatch(website))
                {
                    WebsiteErrorText.Text = "Website must start with http:// or https://";
                    WebsiteErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    WebsiteErrorText.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                WebsiteErrorText.Visibility = Visibility.Collapsed;
            }
        }
        
        ValidateForm();
    }

    private void OnCoordinateChanged(object sender, TextChangedEventArgs e)
    {
        ValidateCoordinates();
        ValidateForm();
    }

    private void ValidateCoordinates()
    {
        var latitude = LatitudeTextBox.Text.Trim();
        var longitude = LongitudeTextBox.Text.Trim();

        // Validate latitude
        if (!string.IsNullOrEmpty(latitude))
        {
            if (!_coordinateRegex.IsMatch(latitude))
            {
                LatitudeErrorText.Text = "Invalid latitude format";
                LatitudeErrorText.Visibility = Visibility.Visible;
            }
            else if (double.TryParse(latitude, out var lat) && (lat < -90 || lat > 90))
            {
                LatitudeErrorText.Text = "Latitude must be between -90 and 90";
                LatitudeErrorText.Visibility = Visibility.Visible;
            }
            else
            {
                LatitudeErrorText.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            LatitudeErrorText.Visibility = Visibility.Collapsed;
        }

        // Validate longitude
        if (!string.IsNullOrEmpty(longitude))
        {
            if (!_coordinateRegex.IsMatch(longitude))
            {
                LongitudeErrorText.Text = "Invalid longitude format";
                LongitudeErrorText.Visibility = Visibility.Visible;
            }
            else if (double.TryParse(longitude, out var lng) && (lng < -180 || lng > 180))
            {
                LongitudeErrorText.Text = "Longitude must be between -180 and 180";
                LongitudeErrorText.Visibility = Visibility.Visible;
            }
            else
            {
                LongitudeErrorText.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            LongitudeErrorText.Visibility = Visibility.Collapsed;
        }
    }

    private void ValidateForm()
    {
        var validationErrors = new List<string>();

        // Required field validation
        if (string.IsNullOrWhiteSpace(StoreNameTextBox.Text))
        {
            StoreNameErrorText.Text = "Store name is required";
            StoreNameErrorText.Visibility = Visibility.Visible;
            validationErrors.Add("Store name is required");
        }
        else
        {
            StoreNameErrorText.Visibility = Visibility.Collapsed;
        }

        if (ChainComboBox.SelectedIndex == -1 && string.IsNullOrWhiteSpace(ChainComboBox.Text))
        {
            ChainErrorText.Text = "Chain is required";
            ChainErrorText.Visibility = Visibility.Visible;
            validationErrors.Add("Chain is required");
        }
        else
        {
            ChainErrorText.Visibility = Visibility.Collapsed;
        }

        if (string.IsNullOrWhiteSpace(SuburbTextBox.Text))
        {
            SuburbErrorText.Text = "Suburb is required";
            SuburbErrorText.Visibility = Visibility.Visible;
            validationErrors.Add("Suburb is required");
        }
        else
        {
            SuburbErrorText.Visibility = Visibility.Collapsed;
        }

        if (StateComboBox.SelectedIndex == -1)
        {
            StateErrorText.Text = "State is required";
            StateErrorText.Visibility = Visibility.Visible;
            validationErrors.Add("State is required");
        }
        else
        {
            StateErrorText.Visibility = Visibility.Collapsed;
        }

        // Add validation errors from field-specific validations
        if (PhoneErrorText.Visibility == Visibility.Visible)
            validationErrors.Add(PhoneErrorText.Text);
        if (WebsiteErrorText.Visibility == Visibility.Visible)
            validationErrors.Add(WebsiteErrorText.Text);
        if (PostcodeErrorText.Visibility == Visibility.Visible)
            validationErrors.Add(PostcodeErrorText.Text);
        if (LatitudeErrorText.Visibility == Visibility.Visible)
            validationErrors.Add(LatitudeErrorText.Text);
        if (LongitudeErrorText.Visibility == Visibility.Visible)
            validationErrors.Add(LongitudeErrorText.Text);

        // Show/hide validation summary
        bool isValid = !validationErrors.Any();
        if (!isValid)
        {
            ValidationErrors.Text = string.Join("\n• ", validationErrors);
            ValidationSummary.Visibility = Visibility.Visible;
        }
        else
        {
            ValidationSummary.Visibility = Visibility.Collapsed;
        }

        // Notify parent about validation status
        ValidationChanged?.Invoke(this, isValid);
    }

    public Place CreatePlaceFromForm()
    {
        var place = new Place
        {
            Name = StoreNameTextBox.Text.Trim()
        };

        // Chain
        if (ChainComboBox.SelectedItem is ComboBoxItem chainItem)
            place.Chain = chainItem.Content?.ToString();
        else if (!string.IsNullOrEmpty(ChainComboBox.Text.Trim()))
            place.Chain = ChainComboBox.Text.Trim();

        // Address information
        if (!string.IsNullOrEmpty(AddressTextBox.Text.Trim()))
            place.Address = AddressTextBox.Text.Trim();

        place.Suburb = SuburbTextBox.Text.Trim();

        if (StateComboBox.SelectedItem is ComboBoxItem stateItem)
            place.State = stateItem.Content?.ToString();

        if (!string.IsNullOrEmpty(PostcodeTextBox.Text.Trim()))
            place.Postcode = PostcodeTextBox.Text.Trim();

        // Contact information
        if (!string.IsNullOrEmpty(PhoneTextBox.Text.Trim()))
            place.Phone = PhoneTextBox.Text.Trim();

        if (!string.IsNullOrEmpty(WebsiteTextBox.Text.Trim()))
            place.Website = WebsiteTextBox.Text.Trim();

        // Operating hours
        if (!string.IsNullOrEmpty(OperatingHoursTextBox.Text.Trim()))
            place.OperatingHours = OperatingHoursTextBox.Text.Trim();

        // Services
        var services = new List<string>();
        foreach (var child in ServicesPanel.Children)
        {
            if (child is CheckBox checkBox && checkBox.IsChecked == true && checkBox.Tag is string service)
            {
                services.Add(service);
            }
        }
        place.Services = services;

        // GPS Coordinates
        if (double.TryParse(LatitudeTextBox.Text.Trim(), out var latitude))
            place.Latitude = latitude;

        if (double.TryParse(LongitudeTextBox.Text.Trim(), out var longitude))
            place.Longitude = longitude;

        return place;
    }

    public bool IsValid()
    {
        ValidateForm();
        return ValidationSummary.Visibility == Visibility.Collapsed;
    }

    public void ClearForm()
    {
        // Basic information
        StoreNameTextBox.Text = "";
        ChainComboBox.SelectedIndex = -1;
        ChainComboBox.Text = "";

        // Location
        AddressTextBox.Text = "";
        SuburbTextBox.Text = "";
        StateComboBox.SelectedIndex = -1;
        PostcodeTextBox.Text = "";

        // Contact
        PhoneTextBox.Text = "";
        WebsiteTextBox.Text = "";

        // Additional
        OperatingHoursTextBox.Text = "";

        // Services checkboxes
        foreach (var child in ServicesPanel.Children)
        {
            if (child is CheckBox checkBox)
                checkBox.IsChecked = false;
        }

        // Coordinates
        LatitudeTextBox.Text = "";
        LongitudeTextBox.Text = "";

        // Clear all error messages
        StoreNameErrorText.Visibility = Visibility.Collapsed;
        ChainErrorText.Visibility = Visibility.Collapsed;
        SuburbErrorText.Visibility = Visibility.Collapsed;
        StateErrorText.Visibility = Visibility.Collapsed;
        PostcodeErrorText.Visibility = Visibility.Collapsed;
        PhoneErrorText.Visibility = Visibility.Collapsed;
        WebsiteErrorText.Visibility = Visibility.Collapsed;
        LatitudeErrorText.Visibility = Visibility.Collapsed;
        LongitudeErrorText.Visibility = Visibility.Collapsed;
        ValidationSummary.Visibility = Visibility.Collapsed;
    }
}
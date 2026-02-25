using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AddPriceRecordViewModel : ViewModelBase
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly PriceRecord? _existingPriceRecord;

    private Item? _selectedItem;
    private Place? _selectedPlace;
    private decimal _price;
    private decimal? _originalPrice;
    private bool _isOnSale;
    private string? _saleDescription;
    private DateTime _dateRecorded = DateTime.Today;
    private DateTime? _validTo;
    private string? _source = "Manual";
    private string? _notes;
    private ObservableCollection<Item> _items;
    private ObservableCollection<Place> _places;

    public AddPriceRecordViewModel(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        PriceRecord? existingPriceRecord = null)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _existingPriceRecord = existingPriceRecord;
        _items = new ObservableCollection<Item>();
        _places = new ObservableCollection<Place>();

        LoadItems();
        LoadPlaces();

        if (existingPriceRecord != null)
        {
            LoadExistingData();
        }

        SaveCommand = new RelayCommand(Save, CanSave);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void LoadExistingData()
    {
        if (_existingPriceRecord == null) return;

        SelectedItem = _items.FirstOrDefault(i => i.Id == _existingPriceRecord.ItemId);
        SelectedPlace = _places.FirstOrDefault(p => p.Id == _existingPriceRecord.PlaceId);
        Price = _existingPriceRecord.Price;
        OriginalPrice = _existingPriceRecord.OriginalPrice;
        IsOnSale = _existingPriceRecord.IsOnSale;
        SaleDescription = _existingPriceRecord.SaleDescription;
        DateRecorded = _existingPriceRecord.DateRecorded;
        ValidTo = _existingPriceRecord.ValidTo;
        Source = _existingPriceRecord.Source;
        Notes = _existingPriceRecord.Notes;
    }

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ObservableCollection<Place> Places
    {
        get => _places;
        set => SetProperty(ref _places, value);
    }

    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public Place? SelectedPlace
    {
        get => _selectedPlace;
        set
        {
            if (SetProperty(ref _selectedPlace, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public decimal? OriginalPrice
    {
        get => _originalPrice;
        set => SetProperty(ref _originalPrice, value);
    }

    public bool IsOnSale
    {
        get => _isOnSale;
        set
        {
            if (SetProperty(ref _isOnSale, value))
            {
                if (value && !OriginalPrice.HasValue)
                {
                    // Suggest original price when marking as sale
                    OriginalPrice = Price * 1.2m;
                }
            }
        }
    }

    public string? SaleDescription
    {
        get => _saleDescription;
        set => SetProperty(ref _saleDescription, value);
    }

    public DateTime DateRecorded
    {
        get => _dateRecorded;
        set
        {
            if (SetProperty(ref _dateRecorded, value) && !ValidTo.HasValue)
            {
                // Default expiry to 7 days after record date
                ValidTo = value.AddDays(7);
            }
        }
    }

    public DateTime? ValidTo
    {
        get => _validTo;
        set => SetProperty(ref _validTo, value);
    }

    public string? Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool IsEditMode => _existingPriceRecord != null;
    public string Title => IsEditMode ? "Edit Price Record" : "Add Price Record";

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public bool? DialogResult { get; private set; }

    private void LoadItems()
    {
        var items = _itemRepository.GetAll().OrderBy(i => i.Name);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    private void LoadPlaces()
    {
        var places = _placeRepository.GetAll().OrderBy(p => p.Name);
        Places.Clear();
        foreach (var place in places)
        {
            Places.Add(place);
        }
    }

    private bool CanSave()
    {
        return SelectedItem != null && 
               SelectedPlace != null && 
               Price > 0;
    }

    private void Save()
    {
        try
        {
            var priceRecord = new PriceRecord
            {
                Id = _existingPriceRecord?.Id ?? Guid.NewGuid().ToString(),
                ItemId = SelectedItem!.Id!,
                PlaceId = SelectedPlace!.Id!,
                Price = Price,
                OriginalPrice = OriginalPrice,
                IsOnSale = IsOnSale,
                SaleDescription = SaleDescription,
                DateRecorded = DateRecorded,
                ValidFrom = DateRecorded,
                ValidTo = ValidTo,
                Source = Source,
                Notes = Notes
            };

            if (IsEditMode)
            {
                _priceRecordRepository.Update(priceRecord);
            }
            else
            {
                _priceRecordRepository.Add(priceRecord);
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            // Error handling would be done via dialog service in a real app
            throw;
        }
    }

    private void Cancel()
    {
        DialogResult = false;
    }
}

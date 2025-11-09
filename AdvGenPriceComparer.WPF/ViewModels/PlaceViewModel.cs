using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class PlaceViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<Place> _places;
    private Place? _selectedPlace;

    public PlaceViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _places = new ObservableCollection<Place>();

        AddPlaceCommand = new RelayCommand(AddPlace);
        EditPlaceCommand = new RelayCommand(EditPlace, CanEditOrDelete);
        DeletePlaceCommand = new RelayCommand(DeletePlace, CanEditOrDelete);
        RefreshCommand = new RelayCommand(LoadPlaces);

        LoadPlaces();
    }

    public ObservableCollection<Place> Places
    {
        get => _places;
        set => SetProperty(ref _places, value);
    }

    public Place? SelectedPlace
    {
        get => _selectedPlace;
        set
        {
            if (SetProperty(ref _selectedPlace, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ICommand AddPlaceCommand { get; }
    public ICommand EditPlaceCommand { get; }
    public ICommand DeletePlaceCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadPlaces()
    {
        try
        {
            Places.Clear();
            var places = _dataService.GetAllPlaces();
            foreach (var place in places)
            {
                Places.Add(place);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load stores: {ex.Message}");
        }
    }

    private void AddPlace()
    {
        _dialogService.ShowInfo("Add Store dialog will be implemented.");
    }

    private void EditPlace()
    {
        if (SelectedPlace == null) return;
        _dialogService.ShowInfo($"Edit Store dialog will be implemented for: {SelectedPlace.Name}");
    }

    private void DeletePlace()
    {
        if (SelectedPlace == null) return;

        var result = _dialogService.ShowConfirmation(
            $"Are you sure you want to delete '{SelectedPlace.Name}'?",
            "Confirm Delete");

        if (result)
        {
            try
            {
                _dataService.Places.Delete(SelectedPlace.Id);
                Places.Remove(SelectedPlace);
                _dialogService.ShowSuccess("Store deleted successfully.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to delete store: {ex.Message}");
            }
        }
    }

    private bool CanEditOrDelete()
    {
        return SelectedPlace != null;
    }
}

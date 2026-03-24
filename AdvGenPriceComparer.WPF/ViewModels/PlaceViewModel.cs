using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class PlaceViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IMediator _mediator;
    private ObservableCollection<Place> _places;
    private Place? _selectedPlace;

    public PlaceViewModel(IDialogService dialogService, IMediator mediator)
    {
        _dialogService = dialogService;
        _mediator = mediator;
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
            var places = _mediator.Send(new GetAllPlacesQuery()).GetAwaiter().GetResult();
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
        var viewModel = new AddStoreViewModel(_mediator, _dialogService);
        var window = new AddStoreWindow(viewModel);

        if (window.ShowDialog() == true)
        {
            LoadPlaces();
        }
    }

    private void EditPlace()
    {
        if (SelectedPlace == null) return;
        
        _dialogService.ShowEditPlaceDialog(SelectedPlace);
        LoadPlaces(); // Refresh the list after editing
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
                var deleteResult = _mediator.Send(new DeletePlaceCommand(SelectedPlace.Id)).GetAwaiter().GetResult();
                if (deleteResult.Success)
                {
                    Places.Remove(SelectedPlace);
                    _dialogService.ShowSuccess("Store deleted successfully.");
                }
                else
                {
                    _dialogService.ShowError($"Failed to delete store: {deleteResult.ErrorMessage}");
                }
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

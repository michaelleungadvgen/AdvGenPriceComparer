# MVVM Integration Guide

This document explains how to integrate the newly created ViewModels and Services with the existing WinUI application.

## Architecture Overview

The MVVM (Model-View-ViewModel) implementation includes:

### 1. ViewModels (`/ViewModels/`)
- **BaseViewModel**: Base class with INotifyPropertyChanged implementation
- **RelayCommand**: Command implementation for binding
- **ItemViewModel**: Handles Add Item functionality with validation
- **PlaceViewModel**: Handles Add Place functionality with validation  
- **MainWindowViewModel**: Main application state and commands
- **MainWindowViewModelLegacy**: Simplified version for direct integration

### 2. Services (`/Services/`)
- **IDialogService / DialogService**: Handles dialog management
- **INotificationService / NotificationService**: Handles notifications
- Both services work with ContentDialog for WinUI 3

### 3. Views (`/Views/`)
- **AddItemView**: XAML view with data binding to ItemViewModel
- **AddPlaceView**: XAML view with data binding to PlaceViewModel
- Both use x:Bind for compile-time binding

### 4. Converters (`/Converters/`)
- **StringToVisibilityConverter**: Converts strings to Visibility for conditional UI

## Integration Steps

### Step 1: Update MainWindow.xaml.cs (Simplified Approach)

Replace the existing MainWindow.xaml.cs content with MVVM pattern:

```csharp
public sealed partial class MainWindow : Window
{
    public MainWindowViewModelLegacy ViewModel { get; }

    public MainWindow()
    {
        this.InitializeComponent();
        ViewModel = new MainWindowViewModelLegacy(this);
    }

    // Replace existing button click handlers with command bindings
    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddItemCommand.Execute(null);
    }

    private void AddPlace_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddPlaceCommand.Execute(null);
    }

    // ... other button handlers
}
```

### Step 2: Update MainWindow.xaml Data Binding

Add data context and bind to ViewModel properties:

```xml
<Window x:Class="AdvGenPriceComparer.Desktop.WinUI.MainWindow"
        DataContext="{x:Bind ViewModel}">
    
    <!-- Update stats cards to bind to ViewModel properties -->
    <TextBlock Text="{x:Bind ViewModel.TotalItems, Mode=OneWay}" />
    <TextBlock Text="{x:Bind ViewModel.TrackedStores, Mode=OneWay}" />
    <TextBlock Text="{x:Bind ViewModel.PriceUpdates, Mode=OneWay}" />
    <TextBlock Text="{x:Bind ViewModel.NetworkUsers, Mode=OneWay}" />
    
    <!-- Convert button clicks to command bindings -->
    <Button Command="{x:Bind ViewModel.AddItemCommand}" Content="Add Item"/>
    <Button Command="{x:Bind ViewModel.AddPlaceCommand}" Content="Add Store"/>
    
</Window>
```

### Step 3: Benefits of MVVM Implementation

#### ✅ **Separation of Concerns**
- **Views**: Pure UI logic in XAML
- **ViewModels**: Presentation logic and state management
- **Models**: Business logic (existing Core models)

#### ✅ **Data Binding**
- **Two-way binding** for form inputs
- **Command binding** for button actions
- **Property change notifications** for real-time UI updates

#### ✅ **Validation**
- **Real-time validation** in ViewModels
- **Error display** bound to UI elements
- **Form validation state** controls dialog buttons

#### ✅ **Testability**
- ViewModels can be unit tested independently
- No UI dependencies in business logic
- Mock services for testing

## Example Usage

### Adding an Item with MVVM

1. **User clicks "Add Item"** → Command executes
2. **ViewModel creates ItemViewModel** → Initializes with validation
3. **DialogService shows AddItemView** → Bound to ItemViewModel
4. **User fills form** → Two-way binding updates ViewModel
5. **Validation runs automatically** → UI shows errors in real-time
6. **User clicks "Add"** → Form validation passes
7. **ViewModel creates Item model** → Calls business logic
8. **Service saves to database** → Updates dashboard stats
9. **NotificationService shows success** → User feedback

### Key Features Demonstrated

```csharp
// Real-time validation
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value); // Triggers validation
}

// Command with validation
public ICommand SaveCommand => new RelayCommand(
    execute: () => SaveItem(),
    canExecute: () => IsValid  // Button disabled when form invalid
);

// Property change notifications
private void ValidateItem()
{
    var validation = CreateItem().ValidateItem();
    IsValid = validation.IsValid;  // UI updates automatically
    ValidationErrors = validation.GetErrorsString();
}
```

## Current Status

### ✅ Completed Components
- **BaseViewModel** with INotifyPropertyChanged
- **RelayCommand** for command binding
- **ItemViewModel** with full validation logic
- **PlaceViewModel** with full validation logic
- **Dialog and Notification services**
- **AddItemView and AddPlaceView** with data binding
- **String to Visibility converter**

### 🔧 Integration Required
- **MainWindow XAML updates** for data binding
- **MainWindow.xaml.cs updates** to use ViewModels
- **Remove existing Controls** (AddItemControl, AddPlaceControl)
- **Test the integration** with actual UI

### 💡 Future Enhancements
- **Dependency Injection** setup in App.xaml.cs
- **Navigation service** for view management
- **Search functionality** with ViewModel binding
- **Advanced validation attributes**

## Benefits Over Current Implementation

| Current Code-Behind | MVVM Pattern |
|-------------------|--------------|
| UI and logic mixed | Clear separation |
| Hard to test | Easily testable |
| Manual validation | Automatic validation |
| Event-driven | Command-driven |
| Tight coupling | Loose coupling |
| Code repetition | Reusable components |

The MVVM implementation provides a much cleaner, more maintainable, and testable architecture while preserving all existing functionality.
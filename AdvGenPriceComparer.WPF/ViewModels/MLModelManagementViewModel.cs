using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class MLModelManagementViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;
    private readonly ModelTrainingService _trainingService;
    private readonly CategoryPredictionService _predictionService;
    private readonly string _modelPath;

    private bool _isBusy;
    private string _statusMessage = string.Empty;
    private TrainingResult? _lastTrainingResult;

    private string _modelStatusText = "No model loaded";
    private Brush _modelStatusColor = Brushes.Gray;
    private string _modelPathText = "Not found";
    private string _modelLastTrained = "Never";
    private string _modelSize = "0 KB";

    private string _testProductName = string.Empty;
    private string _testBrand = string.Empty;
    private string _testDescription = string.Empty;

    private string _predictedCategory = string.Empty;
    private string _predictionConfidence = string.Empty;
    private ObservableCollection<CategoryScoreDisplay> _categoryScores = new();

    public MLModelManagementViewModel(
        IGroceryDataService dataService,
        IDialogService dialogService,
        ILoggerService logger,
        string modelPath)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _logger = logger;
        _modelPath = modelPath;

        _trainingService = new ModelTrainingService(
            null, // IModelVersionService - not used directly in this VM
            msg => _logger.LogInfo(msg),
            (msg, ex) => _logger.LogError(msg, ex),
            msg => _logger.LogWarning(msg));

        _predictionService = new CategoryPredictionService(
            modelPath,
            msg => _logger.LogInfo(msg),
            (msg, ex) => _logger.LogError(msg, ex),
            msg => _logger.LogWarning(msg));

        TrainFromDatabaseCommand = new RelayCommand(async () => await TrainFromDatabaseAsync(), () => IsNotBusy);
        TrainFromCsvCommand = new RelayCommand(async () => await TrainFromCsvAsync(), () => IsNotBusy);
        RetrainModelCommand = new RelayCommand(async () => await RetrainModelAsync(), () => IsNotBusy && CanRetrain);
        ReloadModelCommand = new RelayCommand(ReloadModel, () => IsNotBusy);
        TestPredictionCommand = new RelayCommand(TestPrediction, () => CanTestPrediction);

        LoadModelInfo();
    }

    public ICommand TrainFromDatabaseCommand { get; }
    public ICommand TrainFromCsvCommand { get; }
    public ICommand RetrainModelCommand { get; }
    public ICommand ReloadModelCommand { get; }
    public ICommand TestPredictionCommand { get; }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    public string ModelStatusText
    {
        get => _modelStatusText;
        set => SetProperty(ref _modelStatusText, value);
    }

    public Brush ModelStatusColor
    {
        get => _modelStatusColor;
        set => SetProperty(ref _modelStatusColor, value);
    }

    public string ModelPath
    {
        get => _modelPathText;
        set => SetProperty(ref _modelPathText, value);
    }

    public string ModelLastTrained
    {
        get => _modelLastTrained;
        set => SetProperty(ref _modelLastTrained, value);
    }

    public string ModelSize
    {
        get => _modelSize;
        set => SetProperty(ref _modelSize, value);
    }

    public bool CanRetrain => File.Exists(_modelPath) && _lastTrainingResult?.Success == true;

    public bool HasTrainingResult => _lastTrainingResult != null;

    public string LastTrainingAccuracy => _lastTrainingResult?.Success == true
        ? $"{_lastTrainingResult.Accuracy:P1}"
        : "N/A";

    public string LastTrainingItemCount => _lastTrainingResult?.TrainingItemCount.ToString() ?? "N/A";

    public string LastTrainingDuration => _lastTrainingResult?.Duration != null
        ? $"{_lastTrainingResult.Duration:mm\\:ss}"
        : "N/A";

    public string LastTrainingMessage => _lastTrainingResult?.Message ?? string.Empty;

    public Brush LastTrainingMessageColor => _lastTrainingResult?.Success == true
        ? Brushes.Green
        : Brushes.Red;

    public string TestProductName
    {
        get => _testProductName;
        set
        {
            if (SetProperty(ref _testProductName, value))
            {
                OnPropertyChanged(nameof(CanTestPrediction));
            }
        }
    }

    public string TestBrand
    {
        get => _testBrand;
        set => SetProperty(ref _testBrand, value);
    }

    public string TestDescription
    {
        get => _testDescription;
        set => SetProperty(ref _testDescription, value);
    }

    public bool CanTestPrediction => !string.IsNullOrWhiteSpace(TestProductName) && !IsBusy;

    public bool HasPredictionResult => !string.IsNullOrEmpty(PredictedCategory);

    public string PredictedCategory
    {
        get => _predictedCategory;
        set
        {
            if (SetProperty(ref _predictedCategory, value))
            {
                OnPropertyChanged(nameof(HasPredictionResult));
            }
        }
    }

    public string PredictionConfidence
    {
        get => _predictionConfidence;
        set => SetProperty(ref _predictionConfidence, value);
    }

    public bool HasCategoryScores => CategoryScores.Any();

    public ObservableCollection<CategoryScoreDisplay> CategoryScores
    {
        get => _categoryScores;
        set
        {
            if (SetProperty(ref _categoryScores, value))
            {
                OnPropertyChanged(nameof(HasCategoryScores));
            }
        }
    }

    private void LoadModelInfo()
    {
        try
        {
            var modelInfo = _trainingService.GetModelInfo(_modelPath);

            if (modelInfo != null && modelInfo.IsValid)
            {
                ModelStatusText = "Model Ready";
                ModelStatusColor = Brushes.Green;
                ModelPath = modelInfo.Path;
                ModelLastTrained = $"Last trained: {modelInfo.LastTrained:g}";
                ModelSize = $"Size: {FormatBytes(modelInfo.FileSizeBytes)}";
            }
            else
            {
                ModelStatusText = "No Model Found";
                ModelStatusColor = Brushes.Orange;
                ModelPath = _modelPath;
                ModelLastTrained = "Last trained: Never";
                ModelSize = "Size: N/A";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load model info", ex);
            ModelStatusText = "Error Loading Model";
            ModelStatusColor = Brushes.Red;
        }
    }

    private async Task TrainFromDatabaseAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading items from database...";

            var items = _dataService.GetAllItems().ToList();
            var categorizedItems = items.Where(i => !string.IsNullOrEmpty(i.Category)).ToList();

            if (categorizedItems.Count < ModelTrainingService.MinimumTrainingItems)
            {
                _dialogService.ShowWarning(
                    $"Insufficient training data. Need at least {ModelTrainingService.MinimumTrainingItems} categorized items, " +
                    $"but only found {categorizedItems.Count}.\n\n" +
                    "Please categorize more items or use Train from CSV option.",
                    "Insufficient Data");
                StatusMessage = $"Training cancelled - only {categorizedItems.Count} categorized items available";
                return;
            }

            StatusMessage = $"Training model with {categorizedItems.Count} items...";

            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);

            _lastTrainingResult = await _trainingService.TrainModelFromDatabaseAsync(items, _modelPath);

            OnPropertyChanged(nameof(HasTrainingResult));
            OnPropertyChanged(nameof(LastTrainingAccuracy));
            OnPropertyChanged(nameof(LastTrainingItemCount));
            OnPropertyChanged(nameof(LastTrainingDuration));
            OnPropertyChanged(nameof(LastTrainingMessage));
            OnPropertyChanged(nameof(LastTrainingMessageColor));
            OnPropertyChanged(nameof(CanRetrain));

            if (_lastTrainingResult.Success)
            {
                _dialogService.ShowSuccess(
                    $"Model trained successfully!\n\n" +
                    $"Accuracy: {_lastTrainingResult.Accuracy:P1}\n" +
                    $"Items used: {_lastTrainingResult.TrainingItemCount}\n" +
                    $"Duration: {_lastTrainingResult.Duration:mm\\:ss}",
                    "Training Complete");

                _predictionService.ReloadModel();
                LoadModelInfo();
                StatusMessage = "Training completed successfully";
            }
            else
            {
                _dialogService.ShowError(
                    $"Training failed: {_lastTrainingResult.Message}",
                    "Training Failed");
                StatusMessage = $"Training failed: {_lastTrainingResult.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during training", ex);
            _dialogService.ShowError($"Training error: {ex.Message}", "Training Error");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TrainFromCsvAsync()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Select Training Data CSV"
            };

            if (dialog.ShowDialog() != true)
                return;

            IsBusy = true;
            StatusMessage = "Training model from CSV...";

            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);

            await Task.Run(() =>
            {
                _lastTrainingResult = _trainingService.TrainModelFromCsv(dialog.FileName, _modelPath);
            });

            OnPropertyChanged(nameof(HasTrainingResult));
            OnPropertyChanged(nameof(LastTrainingAccuracy));
            OnPropertyChanged(nameof(LastTrainingItemCount));
            OnPropertyChanged(nameof(LastTrainingDuration));
            OnPropertyChanged(nameof(LastTrainingMessage));
            OnPropertyChanged(nameof(LastTrainingMessageColor));
            OnPropertyChanged(nameof(CanRetrain));

            if (_lastTrainingResult.Success)
            {
                _dialogService.ShowSuccess(
                    $"Model trained successfully from CSV!\n\n" +
                    $"Accuracy: {_lastTrainingResult.Accuracy:P1}\n" +
                    $"Items used: {_lastTrainingResult.TrainingItemCount}\n" +
                    $"Duration: {_lastTrainingResult.Duration:mm\\:ss}",
                    "Training Complete");

                _predictionService.ReloadModel();
                LoadModelInfo();
                StatusMessage = "Training from CSV completed successfully";
            }
            else
            {
                _dialogService.ShowError(
                    $"Training failed: {_lastTrainingResult.Message}",
                    "Training Failed");
                StatusMessage = $"Training failed: {_lastTrainingResult.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during CSV training", ex);
            _dialogService.ShowError($"Training error: {ex.Message}", "Training Error");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RetrainModelAsync()
    {
        try
        {
            if (!_dialogService.ShowQuestion(
                "Retraining will update the existing model with recently categorized items.\n\n" +
                "Do you want to continue?",
                "Confirm Retraining"))
            {
                return;
            }

            IsBusy = true;
            StatusMessage = "Loading new training data...";

            var items = _dataService.GetAllItems()
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .ToList();

            var newTrainingData = items.Select(item => new ProductData
            {
                ProductName = item.Name ?? "",
                Brand = item.Brand ?? "",
                Description = item.Description ?? "",
                Store = "",
                Category = item.Category ?? "Uncategorized"
            }).ToList();

            if (newTrainingData.Count < ModelTrainingService.MinimumItemsPerCategory)
            {
                _dialogService.ShowWarning(
                    $"Insufficient new data. Need at least {ModelTrainingService.MinimumItemsPerCategory} items.",
                    "Insufficient Data");
                StatusMessage = "Retraining cancelled - insufficient new data";
                return;
            }

            StatusMessage = $"Retraining model with {newTrainingData.Count} items...";

            _lastTrainingResult = await _trainingService.RetrainModelAsync(
                _modelPath,
                newTrainingData,
                _modelPath);

            OnPropertyChanged(nameof(HasTrainingResult));
            OnPropertyChanged(nameof(LastTrainingAccuracy));
            OnPropertyChanged(nameof(LastTrainingItemCount));
            OnPropertyChanged(nameof(LastTrainingDuration));
            OnPropertyChanged(nameof(LastTrainingMessage));
            OnPropertyChanged(nameof(LastTrainingMessageColor));

            if (_lastTrainingResult.Success)
            {
                _dialogService.ShowSuccess(
                    $"Model retrained successfully!\n\n" +
                    $"New samples: {newTrainingData.Count}\n" +
                    $"Duration: {_lastTrainingResult.Duration:mm\\:ss}",
                    "Retraining Complete");

                _predictionService.ReloadModel();
                LoadModelInfo();
                StatusMessage = "Retraining completed successfully";
            }
            else
            {
                _dialogService.ShowError(
                    $"Retraining failed: {_lastTrainingResult.Message}",
                    "Retraining Failed");
                StatusMessage = $"Retraining failed: {_lastTrainingResult.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during retraining", ex);
            _dialogService.ShowError($"Retraining error: {ex.Message}", "Retraining Error");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReloadModel()
    {
        try
        {
            StatusMessage = "Reloading model...";

            if (_predictionService.ReloadModel())
            {
                LoadModelInfo();
                _dialogService.ShowSuccess("Model reloaded successfully!", "Success");
                StatusMessage = "Model reloaded successfully";
            }
            else
            {
                _dialogService.ShowWarning("Could not reload model. Model file may not exist.", "Reload Failed");
                StatusMessage = "Failed to reload model";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reloading model", ex);
            _dialogService.ShowError($"Error reloading model: {ex.Message}", "Error");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void TestPrediction()
    {
        try
        {
            StatusMessage = "Testing prediction...";

            var productData = new ProductData
            {
                ProductName = TestProductName,
                Brand = TestBrand,
                Description = TestDescription,
                Store = ""
            };

            var prediction = _predictionService.PredictCategory(productData);

            PredictedCategory = prediction.PredictedCategory;
            PredictionConfidence = $"{prediction.Confidence:P1}";

            var scores = prediction.CategoryScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => new CategoryScoreDisplay
                {
                    Category = kvp.Key,
                    Score = kvp.Value,
                    ScoreText = $"{kvp.Value:P0}",
                    ScorePercentage = (int)(kvp.Value * 100),
                    BarColor = GetScoreColor(kvp.Value)
                })
                .ToList();

            CategoryScores = new ObservableCollection<CategoryScoreDisplay>(scores);

            StatusMessage = $"Predicted '{TestProductName}' as '{PredictedCategory}' with {PredictionConfidence} confidence";
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during prediction test", ex);
            _dialogService.ShowError($"Prediction error: {ex.Message}", "Error");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private static Brush GetScoreColor(float score)
    {
        return score switch
        {
            >= 0.7f => Brushes.Green,
            >= 0.4f => Brushes.Orange,
            _ => Brushes.Gray
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1} {suffixes[counter]}";
    }
}

public class CategoryScoreDisplay
{
    public string Category { get; set; } = string.Empty;
    public float Score { get; set; }
    public string ScoreText { get; set; } = string.Empty;
    public int ScorePercentage { get; set; }
    public Brush BarColor { get; set; } = Brushes.Gray;
}

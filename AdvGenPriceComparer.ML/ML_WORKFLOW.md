# ML.NET Auto-Categorization Workflow Guide

**Version:** 1.0  
**Last Updated:** March 2026  
**Applies to:** AdvGenPriceComparer v1.0+

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Understanding ML Auto-Categorization](#understanding-ml-auto-categorization)
4. [Training the Model](#training-the-model)
5. [Using Auto-Categorization](#using-auto-categorization)
6. [Improving Model Accuracy](#improving-model-accuracy)
7. [Troubleshooting](#troubleshooting)
8. [Best Practices](#best-practices)
9. [Technical Details](#technical-details)

---

## Overview

The AdvGenPriceComparer application uses **ML.NET** (Microsoft's machine learning framework) to automatically categorize grocery products based on their names, brands, and descriptions. This feature saves time during data import and helps maintain consistent categorization across your price database.

### Key Capabilities

- **Auto-Categorization During Import**: Automatically assigns categories to products during JSON/markdown import
- **Smart Suggestions**: Provides real-time category suggestions when manually adding items
- **Continuous Learning**: Model improves as you correct categorizations
- **Confidence Scoring**: Shows prediction confidence to help you decide when to trust auto-categorization

### Supported Categories

The ML model can predict the following 12 categories:

| Category | Examples |
|----------|----------|
| Meat & Seafood | Beef, chicken, fish, pork |
| Dairy & Eggs | Milk, cheese, yogurt, eggs |
| Fruits & Vegetables | Apples, carrots, lettuce, bananas |
| Bakery | Bread, buns, pastries, cakes |
| Pantry Staples | Rice, pasta, flour, canned goods |
| Snacks & Confectionery | Chips, chocolate, biscuits |
| Beverages | Soft drinks, juice, water, coffee |
| Frozen Foods | Frozen vegetables, ice cream, frozen meals |
| Household Products | Cleaning supplies, paper towels |
| Personal Care | Shampoo, soap, toothpaste |
| Baby Products | Baby food, diapers, formula |
| Pet Care | Pet food, treats, litter |
| Health & Wellness | Vitamins, supplements, pharmacy |

---

## Prerequisites

Before using the ML auto-categorization features, ensure you have:

1. **AdvGenPriceComparer v1.0+** installed
2. **Training Data** (optional but recommended):
   - Minimum: 100 categorized products per category
   - Recommended: 500+ categorized products for best accuracy
3. **Sufficient Disk Space**: ~100MB for model files
4. **RAM**: 4GB+ recommended during model training

---

## Understanding ML Auto-Categorization

### How It Works

1. **Text Featurization**: Product names, brands, and descriptions are converted to numerical features
2. **Pattern Recognition**: The ML model learns patterns (e.g., "milk" → Dairy, "chicken" → Meat)
3. **Multi-Class Classification**: The model predicts the most likely category from 13 options
4. **Confidence Scoring**: Each prediction includes a confidence score (0-100%)

### When to Use Auto-Categorization

| Scenario | Recommendation |
|----------|---------------|
| Importing 1000+ products | ✅ Use auto-categorization |
| Importing well-known brands | ✅ Use auto-categorization |
| Importing generic/store brands | ⚠️ Review suggestions |
| Importing specialty/unique items | ⚠️ Lower confidence threshold |
| Manual item entry | ✅ Use smart suggestions |

---

## Training the Model

### Accessing the ML Model Management Window

1. Open AdvGenPriceComparer
2. Click **"ML Model Management"** in the left sidebar
3. The window shows current model status and training options

### Training Methods

#### Method 1: Train from Database (Recommended)

This uses your existing categorized items to train the model.

1. In the **ML Model Management** window, click **"Train from Database"**
2. The system will:
   - Count categorized items in your database
   - Verify minimum training data requirements (100+ per category)
   - Train the model using SDCA (Stochastic Dual Coordinate Ascent) algorithm
   - Display training progress and results
3. Training typically takes 10-60 seconds depending on data size
4. Results shown:
   - **Accuracy**: Percentage of correct predictions on test data
   - **Training Items**: Number of items used for training
   - **Model Path**: Where the trained model is saved

#### Method 2: Train from CSV

Use this if you have external training data in CSV format.

1. Prepare CSV file with columns:
   ```
   ProductName,Brand,Description,Store,Category
   ```
2. Click **"Train from CSV"**
3. Select your CSV file
4. The model will train and save automatically

#### Method 3: Retrain with New Data

Use this to improve the model with recent categorizations.

1. After manually correcting some auto-categorizations, click **"Retrain with New Data"**
2. The model will incorporate recent corrections
3. Accuracy should improve over time

### Understanding Training Results

| Metric | Good | Excellent |
|--------|------|-----------|
| Macro Accuracy | 70-85% | 85-95% |
| Micro Accuracy | 75-90% | 90-95% |
| Per-Category Accuracy | >60% | >80% |

**Note**: Accuracy depends heavily on the quality and quantity of training data.

---

## Using Auto-Categorization

### During Import

When importing JSON or Markdown files:

1. Open **Import Data** from the Data menu
2. Select your file(s)
3. In the import options, check **"Enable Auto-Categorization"**
4. Set your **Confidence Threshold** (default: 70%)
   - Higher threshold (80-90%): Fewer mistakes, more manual review
   - Lower threshold (50-70%): More automation, may need corrections
5. Click **Import**
6. Review the import results to see auto-categorization statistics

### Manual Item Entry with Smart Suggestions

When adding items manually:

1. Open **Add Item** window
2. Start typing the product name
3. As you type, the system shows:
   - **Top 3 category suggestions** with confidence scores
   - Example: "Dairy & Eggs (85%) | Beverages (10%) | Pantry Staples (5%)"
4. Click a suggestion to auto-fill the category
5. Or select manually if suggestions don't match

### Testing Predictions

To test the model without importing:

1. Open **ML Model Management** window
2. Scroll to the **Testing** section
3. Enter a product name in **"Test Product Name"**
4. Click **"Test Prediction"**
5. View the predicted category and confidence score

---

## Improving Model Accuracy

### Strategies for Better Accuracy

1. **Ensure Sufficient Training Data**
   - Minimum 100 items per category
   - Aim for balanced distribution across categories
   - More data = better accuracy

2. **Correct Misclassifications**
   - When auto-categorization is wrong, correct it manually
   - The system tracks corrections for future retraining
   - Every 100 corrections triggers automatic retraining consideration

3. **Use Consistent Naming**
   - "Milk Full Cream 2L" is better than "2L Full Cream Milk"
   - Include brand names when available
   - Avoid abbreviations

4. **Adjust Confidence Threshold**
   - If getting too many wrong predictions: Increase threshold to 80%+
   - If too many items staying uncategorized: Decrease to 60%

5. **Regular Retraining**
   - Retrain monthly if adding new products regularly
   - Retrain after major import sessions
   - Use "Retrain with New Data" for incremental improvements

### Monitoring Accuracy

The **ML Model Management** window shows:
- Current model accuracy
- Number of training items
- Last trained date

Track these metrics over time to ensure the model is improving.

---

## Troubleshooting

### Common Issues

#### Issue: "Insufficient training data" error

**Cause**: Fewer than 100 categorized items in database  
**Solution**: 
- Manually categorize more items
- Import a dataset with pre-categorized items
- Use sample training data to bootstrap

#### Issue: Low prediction accuracy (<60%)

**Cause**: Insufficient or unbalanced training data  
**Solution**:
- Add more training examples for under-represented categories
- Ensure all 13 categories have training examples
- Check for inconsistent categorizations in training data
- Retrain the model

#### Issue: Model not loading

**Cause**: Model file missing or corrupted  
**Solution**:
- Check the model path in Settings
- Retrain the model from database
- Delete old model file and retrain

#### Issue: Suggestions not appearing in Add Item window

**Cause**: Model not loaded or disabled in settings  
**Solution**:
1. Check Settings > ML Settings > "Enable auto-categorization"
2. Verify model is trained (check ML Model Management)
3. Restart the application

#### Issue: Slow predictions during import

**Cause**: Large import with many uncategorized items  
**Solution**:
- Predictions are processed in batch - this is normal
- For 1000+ items, expect 30-60 seconds additional import time
- Consider importing in smaller batches

### Error Messages Reference

| Error | Meaning | Solution |
|-------|---------|----------|
| "ML model not found" | No trained model exists | Train model from database |
| "Insufficient training data" | <100 categorized items | Add more categorized products |
| "Training failed" | Error during model training | Check logs and try again |
| "Prediction error" | Model file corrupted | Delete and retrain model |

---

## Best Practices

### For Importing Data

1. **Start with categorized data**: Import files that already have categories when possible
2. **Review low-confidence predictions**: Check items with <70% confidence
3. **Use preview feature**: Preview import to see auto-categorizations before saving
4. **Batch similar items**: Import similar products together for consistency

### For Manual Entry

1. **Use descriptive names**: "Dairy Farmers Full Cream Milk 2L" vs "Milk"
2. **Include brand information**: Brands help the model identify categories
3. **Check suggestions before saving**: Even high-confidence suggestions should be verified
4. **Be consistent**: Use the same naming conventions as your training data

### For Model Maintenance

1. **Monthly retraining**: Set a calendar reminder to retrain monthly
2. **Track accuracy**: Note accuracy after each retraining
3. **Backup model files**: Keep copies of well-performing models
4. **Document categorization rules**: Maintain team guidelines for edge cases

---

## Technical Details

### Model Architecture

- **Algorithm**: SDCA (Stochastic Dual Coordinate Ascent) Maximum Entropy
- **Features Used**: ProductName, Brand, Description, Store
- **Feature Engineering**: Text featurization with n-grams
- **Output**: Multi-class classification (13 categories)
- **Confidence**: Softmax probability scores

### File Locations

| File | Location | Purpose |
|------|----------|---------|
| Trained Model | `%AppData%\AdvGenPriceComparer\MLModels\category_model.zip` | Serialized ML model |
| Training Data | `AdvGenPriceComparer.ML\Data\sample_training_data.csv` | Sample data for initial training |
| Settings | `%AppData%\AdvGenPriceComparer\settings.json` | ML configuration |

### Performance Specifications

| Operation | Performance |
|-----------|-------------|
| Single prediction | <10ms |
| Batch prediction (100 items) | <500ms |
| Model training (1000 items) | 10-30 seconds |
| Model training (5000 items) | 30-60 seconds |
| Model loading | <1 second |

### Expected Accuracy by Category

Based on sufficient training data:

| Category | Expected Accuracy |
|----------|-------------------|
| Dairy & Eggs | 90-95% |
| Meat & Seafood | 90-95% |
| Beverages | 85-92% |
| Bakery | 85-90% |
| Fruits & Vegetables | 80-88% |
| Pantry Staples | 75-85% |
| Snacks & Confectionery | 75-85% |
| Others | 70-80% |

---

## Quick Reference

### Enabling Auto-Categorization

```
Settings > ML Settings > Enable auto-categorization ✓
```

### Training the Model

```
ML Model Management > Train from Database
```

### Adjusting Confidence Threshold

```
Settings > ML Settings > Confidence Threshold: [0.1 - 0.95]
```

### Testing a Prediction

```
ML Model Management > Testing > Enter product name > Test Prediction
```

### Manual Retraining

```
ML Model Management > Retrain with New Data
```

---

## Support

For additional help with ML auto-categorization:

1. Check the application logs: `%AppData%\AdvGenPriceComparer\Logs\`
2. Review the ML Model Management window for current status
3. Ensure your training data is diverse and well-categorized
4. Consider using the sample training data as a starting point

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | March 2026 | Initial ML workflow documentation |

---

**Note**: This documentation applies to ML.NET auto-categorization features. For price prediction and forecasting features, see the separate Price Prediction documentation.

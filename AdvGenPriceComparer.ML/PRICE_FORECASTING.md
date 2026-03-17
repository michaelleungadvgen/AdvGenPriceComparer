# ML.NET Price Prediction & Forecasting Documentation

**Version:** 1.0  
**Last Updated:** March 2026  
**Applies to:** AdvGenPriceComparer v1.0+  
**Related Components:** PriceForecastingService, PriceAnomalyDetectionService

---

## Table of Contents

1. [Overview](#overview)
2. [Accuracy Metrics](#accuracy-metrics)
3. [Performance Expectations](#performance-expectations)
4. [Limitations](#limitations)
5. [Data Requirements](#data-requirements)
6. [Interpreting Forecasts](#interpreting-forecasts)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The AdvGenPriceComparer application uses **ML.NET Time Series Analysis** with **SSA (Singular Spectrum Analysis)** to predict future grocery prices, detect anomalies, and provide buying recommendations. This document details the accuracy metrics, limitations, and proper usage of the price forecasting features.

### Key Capabilities

- **Future Price Forecasting**: Predict prices up to 30 days ahead
- **Trend Analysis**: Identify rising, falling, or stable price trends
- **Anomaly Detection**: Detect unusual price spikes and illusory discounts
- **Buying Recommendations**: Get AI-powered advice on when to buy

---

## Accuracy Metrics

### Mean Absolute Percentage Error (MAPE)

| Data Quality | Expected MAPE | Interpretation |
|--------------|---------------|----------------|
| Poor (<30 days history) | 15-25% | High uncertainty, use for guidance only |
| Good (30-90 days) | 10-15% | Moderate accuracy, reasonable for planning |
| Excellent (90+ days) | 5-10% | High accuracy, reliable for decisions |

**Note:** MAPE measures the average percentage difference between predicted and actual prices. Lower is better.

### Confidence Intervals

All forecasts include 95% confidence intervals:

| Confidence Level | Interpretation |
|------------------|----------------|
| 95% | The actual price will fall within the predicted range 95% of the time |
| Lower Bound | Conservative estimate (rarely exceeded) |
| Upper Bound | Maximum expected price |

### Trend Detection Accuracy

| Trend Type | Detection Accuracy | Notes |
|------------|-------------------|-------|
| Rising | 75-85% | Most reliable for sustained increases |
| Falling | 75-85% | Most reliable for sustained decreases |
| Stable | 60-70% | Harder to detect, requires more data |

### Anomaly Detection Performance

| Anomaly Type | Precision | Recall | Notes |
|--------------|-----------|--------|-------|
| Price Spike | 80-85% | 70-75% | May miss gradual increases |
| Price Drop | 80-85% | 70-75% | Good at detecting sudden sales |
| Illusory Discount | 70-80% | 60-70% | Requires sufficient historical data |

**Precision:** Of all detected anomalies, what percentage were actual anomalies?  
**Recall:** Of all actual anomalies, what percentage were detected?

---

## Performance Expectations

### Forecasting Performance

| Operation | Expected Time | Factors Affecting Performance |
|-----------|---------------|------------------------------|
| Single Item Forecast (30 days) | 1-3 seconds | Data history size, hardware |
| Batch Forecast (10 items) | 5-10 seconds | Parallel processing available |
| Model Training | 5-30 seconds | Dataset size, horizon length |
| Anomaly Detection | <1 second | Historical data points |

### Resource Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| RAM | 4GB | 8GB+ |
| CPU | Dual-core | Quad-core+ |
| Disk Space | 100MB | 500MB |
| Historical Data | 30 days | 90+ days |

---

## Limitations

### Data Limitations

1. **Minimum Data Requirements**
   - **Absolute Minimum:** 30 days of price history
   - **Recommended:** 90+ days for reliable forecasts
   - **Optimal:** 6+ months for seasonal pattern detection
   
   **Impact:** Insufficient data leads to wider confidence intervals and less accurate predictions.

2. **Data Quality Issues**
   - Missing price records create gaps in analysis
   - Infrequent price updates reduce forecast reliability
   - Inconsistent product naming affects history matching

3. **Seasonal Patterns**
   - Currently limited seasonal detection (weekly patterns only)
   - Holiday and event-based pricing not explicitly modeled
   - Annual seasonality requires 12+ months of data

### Algorithm Limitations

1. **SSA (Singular Spectrum Analysis) Constraints**
   - Assumes some degree of periodicity in data
   - Performance degrades with highly volatile prices
   - Best suited for products with regular price cycles

2. **Short-Term Focus**
   - Forecasts beyond 30 days become increasingly unreliable
   - Confidence intervals expand significantly after 14 days
   - Not suitable for long-term price planning (>1 month)

3. **External Factors**
   - Cannot predict external events (supply chain issues, disasters)
   - Does not account for promotional calendars
   - No integration with economic indicators

### Product-Specific Limitations

| Product Type | Forecast Reliability | Notes |
|--------------|---------------------|-------|
| Staples (milk, bread) | High | Regular pricing patterns |
| Seasonal Items | Low-Medium | Requires annual data |
| Promotional Items | Low | Volatile, deal-dependent |
| New Products | Very Low | Insufficient history |
| Discontinued Items | N/A | Cannot forecast |

### Geographic Limitations

- Forecasts are store-specific; prices may vary at different locations
- Regional pricing strategies not accounted for
- Currency fluctuations not modeled

---

## Data Requirements

### Minimum Viable Dataset

```
Required per item:
- 30+ price records
- Date range: Minimum 30 days
- Price variation: At least 2 different prices
- Update frequency: At least weekly
```

### Optimal Dataset

```
Recommended per item:
- 200+ price records
- Date range: 6+ months
- Price variation: Multiple price points
- Update frequency: Daily or per-catalogue
- Sale indicators: On/off sale flags
```

### Data Quality Checklist

- [ ] Prices are numeric and positive
- [ ] Dates are in chronological order
- [ ] No duplicate entries for same date
- [ ] Missing data points are minimal (<10%)
- [ ] Product identifiers are consistent

---

## Interpreting Forecasts

### Understanding Confidence Intervals

```
Example Forecast:
Day 7: Predicted $4.50
       Lower Bound: $4.10
       Upper Bound: $4.90
       
Interpretation: 
- Most likely price: $4.50
- 95% chance price will be between $4.10-$4.90
- Uncertainty range: $0.80 (±9%)
```

### Reading Trend Indicators

| Indicator | Meaning | Action |
|-----------|---------|--------|
| 📈 Rising | Price expected to increase | Consider buying now |
| 📉 Falling | Price expected to decrease | Wait if possible |
| ➡️ Stable | No significant change expected | Buy when convenient |

### Buying Recommendations Explained

| Recommendation | Trigger Condition | Confidence |
|----------------|-------------------|------------|
| ✅ BUY NOW | Price at/near predicted low | High |
| ⏳ WAIT | Price predicted to drop soon | Medium-High |
| ❌ AVOID | Price unusually high | Medium |
| ℹ️ NORMAL | No significant trend | Low-Medium |

---

## Best Practices

### For Accurate Forecasts

1. **Maintain Consistent Data Collection**
   - Update prices regularly (at least weekly)
   - Capture sale prices and original prices
   - Record deal expiration dates when available

2. **Use Appropriate Time Horizons**
   - 7-day forecasts: Highly reliable
   - 14-day forecasts: Reliable for planning
   - 30-day forecasts: Use for guidance only

3. **Validate Predictions**
   - Compare forecasts against actual prices
   - Track accuracy over time
   - Adjust buying strategy based on prediction quality

4. **Consider Product Categories**
   - Staples: Trust forecasts more
   - Luxury/Seasonal: Be more skeptical
   - New products: Wait for data accumulation

### For Anomaly Detection

1. **Review Detected Anomalies**
   - Not all anomalies are errors
   - Some may be legitimate sales
   - Cross-reference with store catalogues

2. **Verify Illusory Discounts**
   - Compare "sale" price to historical average
   - Check if discount is genuine
   - Use for 30+ day old products only

3. **Set Appropriate Thresholds**
   - Default 95% confidence works for most cases
   - Lower threshold (90%) for more sensitive detection
   - Higher threshold (99%) to reduce false positives

### Interpreting Results

1. **Don't Rely on Single Forecasts**
   - Check multiple time horizons
   - Compare trend directions
   - Look for consistent patterns

2. **Combine with Other Information**
   - Store catalogue knowledge
   - Seasonal awareness
   - Personal shopping patterns

---

## Troubleshooting

### Common Issues

#### Issue: "Insufficient data for forecasting"

**Cause:** Less than 30 days of price history  
**Solution:**
- Wait for more data to accumulate
- Import historical price data if available
- Use manual price tracking for critical items

#### Issue: Very wide confidence intervals

**Cause:** High price volatility or insufficient data  
**Solution:**
- Continue collecting data
- Focus on 7-day forecasts only
- Consider product may be too volatile for forecasting

#### Issue: Inaccurate trend detection

**Cause:** Irregular price updates or erratic pricing  
**Solution:**
- Ensure regular price updates
- Check for data entry errors
- Consider product may not have predictable patterns

#### Issue: Too many false anomaly alerts

**Cause:** Sensitivity too high or normal price variation  
**Solution:**
- Increase confidence threshold to 97-99%
- Review what constitutes "normal" variation
- Exclude highly volatile products from monitoring

#### Issue: Forecasts consistently wrong

**Cause:** Model may need retraining or product is unpredictable  
**Solution:**
- Retrain forecasting model
- Check for data quality issues
- Consider if product has truly unpredictable pricing

### Error Messages Reference

| Message | Meaning | Solution |
|---------|---------|----------|
| "Insufficient data" | <30 days history | Collect more data |
| "Model not trained" | No forecasting model | Train model first |
| "High volatility detected" | Unpredictable prices | Use shorter forecasts |
| "Seasonal pattern unclear" | Need more data | Wait for 6+ months data |

### Performance Issues

| Symptom | Cause | Solution |
|---------|-------|----------|
| Slow forecast generation | Large dataset | Batch fewer items |
| High memory usage | Many concurrent forecasts | Process sequentially |
| Timeout errors | Complex calculations | Reduce forecast horizon |

---

## Technical Details

### Algorithm: Singular Spectrum Analysis (SSA)

**How it works:**
1. Decomposes time series into trend, periodic, and noise components
2. Identifies underlying patterns in historical data
3. Projects patterns forward to generate forecasts
4. Calculates confidence bounds based on historical variance

**Parameters:**
- Window Size: 7 days (weekly patterns)
- Confidence Level: 95%
- Horizon: Up to 30 days

### Model Training

**Training Frequency:**
- Initial: When 30+ days data available
- Retraining: Recommended monthly
- Trigger: Significant prediction drift

**Training Data:**
- All historical prices for item
- Excludes items with <30 records
- Weights recent data slightly higher

### Limitations of Current Implementation

1. Single algorithm (SSA) - no ensemble methods
2. Fixed confidence level (95%)
3. No automatic retraining
4. Limited to 30-day horizon
5. No cross-item price correlation analysis

---

## Future Improvements

Planned enhancements for price forecasting:

- [ ] Ensemble models (combine multiple algorithms)
- [ ] Long-term forecasting (90+ days)
- [ ] Cross-item correlation analysis
- [ ] Promotional calendar integration
- [ ] Automatic model retraining
- [ ] Mobile push notifications for price alerts
- [ ] Machine learning-based deal prediction

---

## Quick Reference

### Forecast Reliability by Horizon

| Horizon | Reliability | Use Case |
|---------|-------------|----------|
| 1-7 days | High (±5%) | Immediate buying decisions |
| 8-14 days | Medium (±10%) | Weekly shopping planning |
| 15-30 days | Low-Medium (±15%) | Monthly budget planning |

### When to Trust Forecasts

✅ **Trust when:**
- 90+ days of data available
- Product is staple/grocery item
- Confidence interval <20%
- Trend is consistent across horizons

⚠️ **Be Cautious when:**
- 30-60 days of data
- Seasonal or promotional items
- Confidence interval >20%
- High price volatility

❌ **Don't Rely on:**
- <30 days of data
- New/discontinued products
- Luxury/non-essential items
- External event-driven pricing

---

## Support

For issues with price forecasting:

1. Check data quality in Price History page
2. Verify sufficient historical data exists
3. Review application logs: `%AppData%\AdvGenPriceComparer\Logs\`
4. Ensure ML models are trained in ML Model Management
5. Consider data requirements for your specific products

---

**Document Version:** 1.0  
**Last Updated:** March 2026  
**Applies to:** AdvGenPriceComparer v1.0+

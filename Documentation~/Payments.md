# Yandex Games In-App Purchases

Integrate Yandex Games payment system into your Unity game for WebGL platform.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Best Practices](#best-practices)
- [Common Patterns](#common-patterns)
- [Troubleshooting](#troubleshooting)

## Prerequisites

- **Unity Version**: 2022.3 LTS or newer
- **Platform**: WebGL builds only
- **Dependencies**: UniTask package (Cysharp.Threading.Tasks)
- **Yandex Console**: Products configured in [Yandex Games Console](https://yandex.ru/games/console)
- **Approval**: In-app purchases must be enabled by Yandex support (contact: games-partners@yandex-team.ru)

## Quick Start

### 1. Display Product Catalog

```csharp
using YandexGames;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    async void Start()
    {
        // Wait for SDK initialization
        await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
        
        // Fetch product catalog
        try
        {
            YandexProduct[] products = await YandexGames.YandexGames.Payments.GetCatalogAsync();
            
            foreach (var product in products)
            {
                Debug.Log($"{product.Title}: {product.Price}");
                CreateProductButton(product);
            }
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to load catalog: {ex.Message}");
        }
    }
}
```

### 2. Handle Purchase

```csharp
public async void OnBuyButtonClick(string productId)
{
    try
    {
        // Initiate purchase (opens Yandex payment dialog)
        YandexPurchase purchase = await YandexGames.YandexGames.Payments.PurchaseAsync(productId);
        
        // CRITICAL: Grant items BEFORE consuming
        await GrantItemsToPlayer(productId);
        
        // Consume purchase to remove from unconsumed list
        await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);
        
        ShowSuccessMessage("Purchase complete!");
    }
    catch (YandexGamesException ex)
    {
        Debug.LogWarning($"Purchase failed: {ex.Message}");
        ShowErrorMessage("Purchase cancelled. Please try again.");
    }
}

async UniTask GrantItemsToPlayer(string productId)
{
    switch (productId)
    {
        case "gold500":
            PlayerData.AddGold(500);
            await PlayerData.SaveAsync(); // Save before consuming!
            break;
    }
}
```

### 3. Process Unconsumed Purchases on Startup (REQUIRED)

**⚠️ CRITICAL**: This is **required** for Yandex moderation approval.

```csharp
public class GameManager : MonoBehaviour
{
    async void Start()
    {
        await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
        
        // REQUIRED: Check for unconsumed purchases
        await ProcessUnconsumedPurchases();
    }
    
    async UniTask ProcessUnconsumedPurchases()
    {
        try
        {
            YandexPurchase[] purchases = await YandexGames.YandexGames.Payments.GetPurchasesAsync();
            
            foreach (var purchase in purchases)
            {
                // Permanent purchases - don't consume
                if (purchase.ProductID == "disable_ads")
                {
                    AdManager.DisableAds();
                    continue; // Skip consumption
                }
                
                // Consumable purchases - grant items and consume
                await GrantItemsToPlayer(purchase.ProductID);
                await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);
            }
        }
        catch (YandexGamesException ex)
        {
            if (ex.Message.Contains("USER_NOT_AUTHORIZED"))
            {
                Debug.Log("Player not authorized, skipping purchase check");
            }
            else
            {
                Debug.LogWarning($"Could not check purchases: {ex.Message}");
            }
        }
    }
}
```

## API Reference

### YandexGames.Payments

Access point for all payment operations.

#### GetCatalogAsync()

Retrieves product catalog from Yandex Games.

```csharp
public async UniTask<YandexProduct[]> GetCatalogAsync()
```

**Returns**: Array of products configured in Yandex Console

**Throws**:
- `YandexGamesException` - SDK not initialized, network error, or catalog unavailable

**Example**:
```csharp
YandexProduct[] products = await YandexGames.YandexGames.Payments.GetCatalogAsync();
foreach (var product in products)
{
    Debug.Log($"{product.Id}: {product.Price}");
}
```

#### PurchaseAsync(productId, developerPayload)

Initiates purchase flow for specified product.

```csharp
public async UniTask<YandexPurchase> PurchaseAsync(string productId, string developerPayload = null)
```

**Parameters**:
- `productId` (string, required) - Product identifier from catalog
- `developerPayload` (string, optional) - Custom metadata to attach to purchase

**Returns**: Purchase object with product ID, token, and payload

**Throws**:
- `ArgumentException` - productId is null or empty
- `YandexGamesException` - SDK not initialized, product not found, payment cancelled, network error

**Example**:
```csharp
YandexPurchase purchase = await YandexGames.YandexGames.Payments.PurchaseAsync("gold500");
await GrantItems(purchase.ProductID);
await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);
```

#### GetPurchasesAsync()

Retrieves list of unconsumed purchases.

```csharp
public async UniTask<YandexPurchase[]> GetPurchasesAsync()
```

**Returns**: Array of unconsumed purchases for current player

**Throws**:
- `YandexGamesException` - SDK not initialized, player not authorized (USER_NOT_AUTHORIZED), network error

**Example**:
```csharp
YandexPurchase[] purchases = await YandexGames.YandexGames.Payments.GetPurchasesAsync();
foreach (var purchase in purchases)
{
    await ProcessPurchase(purchase);
}
```

#### ConsumePurchaseAsync(purchaseToken)

Marks a consumable purchase as used.

```csharp
public async UniTask ConsumePurchaseAsync(string purchaseToken)
```

**Parameters**:
- `purchaseToken` (string, required) - Unique token from Purchase object

**Throws**:
- `ArgumentException` - purchaseToken is null or empty
- `YandexGamesException` - SDK not initialized, token invalid/not found, token already consumed

**⚠️ WARNING**: After consumption, purchase is **permanently removed**. Always grant items BEFORE calling this method.

**Example**:
```csharp
await PlayerData.AddGold(500);
await PlayerData.SaveAsync(); // Save first!
await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);
```

#### HasPurchase(productId)

Convenience method to check for permanent purchases.

```csharp
public async UniTask<bool> HasPurchase(string productId)
```

**Parameters**:
- `productId` (string, required) - Product ID to check

**Returns**: True if product exists in unconsumed purchases, false otherwise (including errors)

**Example**:
```csharp
bool hasNoAds = await YandexGames.YandexGames.Payments.HasPurchase("disable_ads");
if (hasNoAds)
{
    AdManager.DisableAds();
}
```

### Data Models

#### YandexProduct

Represents a product in the catalog.

```csharp
public class YandexProduct
{
    public string Id { get; }              // Product identifier
    public string Title { get; }           // Localized title
    public string Description { get; }     // Localized description
    public string ImageURI { get; }        // Product image URL
    public string Price { get; }           // Formatted price (e.g., "50 YAN")
    public string PriceValue { get; }      // Numeric value (e.g., "50")
    public string PriceCurrencyCode { get; } // Currency code (e.g., "YAN")
    
    // Get currency icon URL
    public string GetPriceCurrencyImage(CurrencyIconSize size);
}

public enum CurrencyIconSize
{
    Small,   // 16x16 icon
    Medium,  // 32x32 icon
    Svg      // Vector format
}
```

#### YandexPurchase

Represents a purchase transaction.

```csharp
public class YandexPurchase
{
    public string ProductID { get; }        // Product identifier
    public string PurchaseToken { get; }    // Unique token for consumption
    public string DeveloperPayload { get; } // Custom data from PurchaseAsync()
    public string Signature { get; }        // HMAC-SHA256 signature (signed mode only)
}
```

## Best Practices

### 1. Always Check Unconsumed Purchases on Startup

This is **required** for Yandex moderation. Prevents lost revenue if game crashes during purchase processing.

```csharp
async void Start()
{
    await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
    await ProcessUnconsumedPurchases(); // REQUIRED
}
```

### 2. Grant Items BEFORE Consuming Purchases

```csharp
// ✅ CORRECT: Grant items first
await PlayerData.AddGold(500);
await PlayerData.SaveAsync();
await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(token);

// ❌ WRONG: Don't consume before granting
await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(token);
await PlayerData.AddGold(500); // Crash here = lost revenue!
```

### 3. Handle Permanent vs Consumable Purchases

**Consumable** (coins, lives): Grant items and consume
**Permanent** (ad removal, unlocks): Check presence, don't consume

```csharp
if (purchase.ProductID == "disable_ads")
{
    AdManager.DisableAds();
    // Don't consume - check on every startup
}
else
{
    await GrantItems(purchase.ProductID);
    await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);
}
```

### 4. Handle Unauthorized Players Gracefully

```csharp
try
{
    var purchases = await YandexGames.YandexGames.Payments.GetPurchasesAsync();
}
catch (YandexGamesException ex)
{
    if (ex.Message.Contains("USER_NOT_AUTHORIZED"))
    {
        // Expected for guest players - don't show error
        Debug.Log("Player not authorized");
    }
    else
    {
        Debug.LogError($"Unexpected error: {ex.Message}");
    }
}
```

### 5. Use Try-Catch for All Payment Operations

Payment operations can fail due to network issues, user cancellation, or insufficient funds.

```csharp
try
{
    var purchase = await YandexGames.YandexGames.Payments.PurchaseAsync("gold500");
    // Process purchase
}
catch (YandexGamesException ex)
{
    // Show user-friendly error message
    ShowMessage("Purchase failed. Please try again.");
}
```

## Common Patterns

### Complete Shop System

```csharp
public class ShopManager : MonoBehaviour
{
    async void Start()
    {
        await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
        await LoadShop();
    }
    
    async UniTask LoadShop()
    {
        try
        {
            YandexProduct[] products = await YandexGames.YandexGames.Payments.GetCatalogAsync();
            DisplayProducts(products);
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to load shop: {ex.Message}");
            ShowRetryButton();
        }
    }
    
    public async void OnPurchaseClick(string productId)
    {
        ShowLoadingIndicator();
        
        try
        {
            YandexPurchase purchase = await YandexGames.YandexGames.Payments.PurchaseAsync(productId);
            
            await GrantItemsToPlayer(productId);
            await YandexGames.YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);
            
            HideLoadingIndicator();
            ShowSuccessAnimation();
        }
        catch (YandexGamesException ex)
        {
            HideLoadingIndicator();
            ShowErrorMessage("Purchase failed");
            Debug.LogWarning($"Purchase error: {ex.Message}");
        }
    }
}
```

### Permanent Purchase Check

```csharp
public class AdManager : MonoBehaviour
{
    private bool _adsDisabled = false;
    
    async void Start()
    {
        await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
        
        // Check if player purchased ad removal
        _adsDisabled = await YandexGames.YandexGames.Payments.HasPurchase("disable_ads");
    }
    
    public void ShowAd()
    {
        if (_adsDisabled)
        {
            Debug.Log("Ads disabled - skipping");
            return;
        }
        
        // Show ad
    }
}
```

## Troubleshooting

### "Plugin not initialized" Error

**Cause**: Calling payment methods before YandexGames SDK initialization completes.

**Solution**: Wait for initialization:
```csharp
await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
```

### "USER_NOT_AUTHORIZED" Error

**Cause**: Player is not logged into Yandex account.

**Solution**: This is expected for guest players. Handle gracefully:
```csharp
catch (YandexGamesException ex)
{
    if (ex.Message.Contains("USER_NOT_AUTHORIZED"))
    {
        // Don't show error to user
        Debug.Log("Guest player - no purchases available");
    }
}
```

### Purchase Completed But Items Not Granted

**Cause**: Game crashed between purchase and consumption, or items not saved.

**Solution**: Implement unconsumed purchase recovery on startup (required):
```csharp
async void Start()
{
    await ProcessUnconsumedPurchases();
}
```

### Product Not Found in Catalog

**Cause**: Product not configured in Yandex Console, or console sync delay.

**Solution**:
1. Check [Yandex Games Console](https://yandex.ru/games/console) configuration
2. Product IDs are case-sensitive
3. Wait 5-10 minutes for console changes to sync

### Editor Testing

In Unity Editor, payment APIs return mock data:
- `GetCatalogAsync()` returns 3 sample products
- `PurchaseAsync()` creates mock purchase with fake token
- `GetPurchasesAsync()` returns mock purchase list
- `ConsumePurchaseAsync()` removes from mock list

Deploy WebGL build to Yandex platform for real payment testing.

## Additional Resources

- [Yandex Games Documentation](https://yandex.ru/dev/games/doc/ru/sdk/sdk-purchases)
- [Yandex Games Console](https://yandex.ru/games/console)
- [UniTask Documentation](https://github.com/Cysharp/UniTask)

## Support

For Yandex payment integration issues:
- Email: games-partners@yandex-team.ru
- Documentation: https://yandex.ru/dev/games/doc/ru/

For plugin issues:
- GitHub Issues: [YandexGames repository]

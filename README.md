# Yandex Games Unity Plugin

A Unity plugin for Yandex Games integration with minimal usage pattern. Provides auto-initialization, player data retrieval, cloud storage, advertisements, leaderboards, and in-app purchases using UniTask for async operations.

## Features

- **Zero Configuration**: Automatically initializes when your game starts
- **Player Data**: Get player information (name, avatar, language, device type)
- **Cloud Storage**: Save and load game data to Yandex Games cloud
- **Advertisements**: Show interstitial and rewarded video ads
- **Leaderboards**: Submit scores, retrieve rankings, and display player positions
- **In-App Purchases**: Display product catalog, process payments, and handle unconsumed purchases
- **Remote Configuration**: Feature flags and A/B testing support
- **Game Review**: Request player feedback and ratings
- **UniTask Integration**: All async operations use UniTask for better performance
- **WebGL Support**: Designed specifically for Unity WebGL builds
- **Editor Testing**: Mock implementations with PlayerPrefs storage for testing in Unity Editor

## Installation

### Via Unity Package Manager

1. Open Unity Package Manager
2. Click the "+" button and select "Add package from git URL"
3. Enter: `https://github.com/yandex-games/unity-plugin.git`

### Manual Installation

1. Download the latest release
2. Extract to your `Assets/` folder or `Packages/` folder

## Quick Start

The plugin follows a "minimal usage" pattern - just add it to your project and it works automatically:

```csharp
using UnityEngine;
using YandexGames;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        // Plugin auto-initializes, just wait for it
        await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
        
        // Get player data
        var player = await YandexGames.YandexGames.GetPlayerDataAsync();
        Debug.Log($"Welcome, {player.name}!");
        
        // Load saved data
        var saveData = await YandexGames.YandexGames.LoadDataAsync("progress");
        if (saveData != null)
        {
            // Restore game state
        }
    }
    
    public async void SaveGame()
    {
        var gameData = JsonUtility.ToJson(new { level = 5, score = 1000 });
        await YandexGames.YandexGames.SaveDataAsync("progress", gameData);
    }
    
    public async void ShowAd()
    {
        await YandexGames.YandexGames.ShowInterstitialAdAsync();
    }
    
    public async void ShowRewardedAd()
    {
        await YandexGames.YandexGames.ShowRewardedAdAsync(rewarded => {
            if (rewarded) {
                // Give player reward
                GivePlayerCoins(100);
            }
        });
    }
}
```

## API Reference

### Core Methods

- `YandexGames.YandexGames.IsInitialized` - Check if plugin is ready
- `YandexGames.YandexGames.GetPlayerDataAsync()` - Get player information
- `YandexGames.YandexGames.SaveDataAsync(key, data)` - Save data to cloud storage (uses PlayerPrefs in Editor)
- `YandexGames.YandexGames.LoadDataAsync(key)` - Load data from cloud storage (uses PlayerPrefs in Editor)
- `YandexGames.YandexGames.ShowInterstitialAdAsync()` - Show interstitial advertisement
- `YandexGames.YandexGames.ShowRewardedAdAsync(callback)` - Show rewarded video advertisement

### Leaderboards

- `SetLeaderboardScoreAsync(leaderboardName, score, extraData)` - Submit player score
- `GetLeaderboardDescriptionAsync(leaderboardName)` - Get leaderboard metadata (title, format, sort order)
- `GetLeaderboardPlayerEntryAsync(leaderboardName)` - Get current player's rank and score
- `GetLeaderboardEntriesAsync(leaderboardName, includeUser, quantityAround, quantityTop)` - Get leaderboard entries

**Example:**
```csharp
// Submit score
await YandexGames.SetLeaderboardScoreAsync("MyLeaderboard", 1000);

// Get top 10 with current player
var response = await YandexGames.GetLeaderboardEntriesAsync(
    "MyLeaderboard", 
    includeUser: true, 
    quantityTop: 10
);

foreach (var entry in response.entries)
{
    Debug.Log($"#{entry.rank + 1}: {entry.player.publicName} - {entry.formattedScore}");
}
```

### In-App Purchases

- `YandexGames.Payments.GetCatalogAsync()` - Get product catalog with prices
- `YandexGames.Payments.PurchaseAsync(productId, developerPayload)` - Initiate purchase flow
- `YandexGames.Payments.GetPurchasesAsync()` - Get unconsumed purchases (required on startup)
- `YandexGames.Payments.ConsumePurchaseAsync(purchaseToken)` - Mark purchase as consumed
- `YandexGames.Payments.HasPurchase(productId)` - Check for permanent purchase

**Example:**
```csharp
// Display shop
var products = await YandexGames.Payments.GetCatalogAsync();
foreach (var product in products)
{
    Debug.Log($"{product.Title}: {product.Price}");
}

// Process purchase
var purchase = await YandexGames.Payments.PurchaseAsync("gold500");
await PlayerData.AddGold(500);
await YandexGames.Payments.ConsumePurchaseAsync(purchase.PurchaseToken);

// Check for permanent purchase
bool hasNoAds = await YandexGames.Payments.HasPurchase("disable_ads");
```

**📖 See [Payments Documentation](Documentation~/Payments.md) for complete guide with best practices.**

### Remote Configuration

- `GetFlagsAsync(defaultFlags, clientFeatures)` - Get feature flags for A/B testing and remote config

**Example:**
```csharp
var defaults = new Dictionary<string, string>
{
    { "newFeature", "false" },
    { "difficulty", "normal" }
};

var flags = await YandexGames.GetFlagsAsync(defaults);
if (flags["newFeature"] == "true")
{
    // Enable new feature
}
```

### Game Review

- `CanReviewAsync()` - Check if review request is available
- `RequestReviewAsync()` - Request user to review the game (once per session)

**Example:**
```csharp
var (canReview, reason) = await YandexGames.CanReviewAsync();
if (canReview)
{
    var feedbackSent = await YandexGames.RequestReviewAsync();
    Debug.Log($"User submitted feedback: {feedbackSent}");
}
else
{
    Debug.Log($"Cannot review: {reason}"); // NO_AUTH, GAME_RATED, etc.
}
```

**Note**: The static class `YandexGames` is inside the `YandexGames` namespace, so you need to use `YandexGames.YandexGames` to access the methods.

### Data Types

```csharp
// Player data
public class PlayerData
{
    public string name;    // Player's display name
    public string avatar;  // Avatar URL
    public string lang;    // Language code (e.g., "en", "ru")
    public string device;  // Device type: "desktop", "mobile", "tablet"
}

// Leaderboard entry
public class LeaderboardEntry
{
    public int score;              // Raw score value
    public int rank;               // 0-based rank (0 = 1st place)
    public string formattedScore;  // Formatted with decimal offset
    public string extraData;       // Custom metadata
    public LeaderboardPlayer player;
}

// Leaderboard player info
public class LeaderboardPlayer
{
    public string uniqueID;
    public string publicName;      // "User Hidden" if privacy restricted
    public string lang;
    public ScopePermissions scopePermissions;
}

// Leaderboard description
public class LeaderboardDescription
{
    public string name;            // Technical name
    public LocalizedTitles title;  // Multi-language titles
    public DescriptionConfig description; // Score format and sort order
}

// Remote config feature
public class ClientFeature
{
    public string name;
    public string value;
    public ClientFeature(string name, string value);
}
```

## Setup for Yandex Games

1. Build your Unity project for WebGL
2. Include the Yandex Games SDK in your HTML template:

```html
<script src="https://yandex.ru/games/sdk/v2"></script>
```

3. Upload your game to Yandex Games platform

## Architecture

The plugin uses a clean architecture with JavaScript-C# communication:

1. **Static API**: The main `YandexGames.YandexGames` static class provides all functionality
2. **Auto-initialization**: Plugin automatically initializes when the game starts using `RuntimeInitializeOnLoadMethod`
3. **Callback Receiver**: A hidden GameObject (`YandexGamesCallbackReceiver`) handles JavaScript-to-C# callbacks
4. **JavaScript Bridge**: `.jslib` files handle Unity WebGL communication with Yandex Games SDK
5. **Async Pattern**: All operations use UniTask for high-performance async operations

### Initialization Behavior

**WebGL Builds**: Initialization is asynchronous. The plugin waits for the Yandex Games SDK JavaScript library to load and confirm successful initialization before setting the `IsInitialized` flag to `true`. This prevents race conditions where API calls might be made before the SDK is ready.

```csharp
// Recommended pattern: Wait for initialization before calling APIs
await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);

// Now safe to call any Yandex API methods
var playerData = await YandexGames.YandexGames.GetPlayerDataAsync();
```

**Unity Editor**: Initialization is immediate (synchronous) to enable development and testing without WebGL builds. Mock implementations are used for all API calls.

**Timeout**: If the JavaScript SDK doesn't respond within 10 seconds, an error will be logged and `IsInitialized` will remain `false`.

The messaging flow:
- C# calls JavaScript functions via `DllImport`
- JavaScript calls Yandex Games SDK (asynchronous Promise-based API)
- When SDK initialization completes, JavaScript uses `SendMessage` to call back to C# via the callback receiver GameObject
- Callback receiver forwards calls to the static class methods, which set the `IsInitialized` flag
- This ensures `IsInitialized` accurately reflects actual SDK readiness state

## Requirements

- Unity 2022.3 LTS or newer
- UniTask package (automatically included as dependency)
- WebGL platform target for production builds

## Error Handling

All methods can throw `YandexGamesException` on errors:

```csharp
try
{
    var playerData = await YandexGames.GetPlayerDataAsync();
}
catch (YandexGamesException ex)
{
    Debug.LogError($"Yandex Games error: {ex.Message}");
}
```

## License

MIT License - see LICENSE file for details

## Support

For issues and questions:
- GitHub Issues: [Report a bug](https://github.com/yandex-games/unity-plugin/issues)
- Yandex Games Documentation: [Official Docs](https://yandex.ru/dev/games/)
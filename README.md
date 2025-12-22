# Yandex Games Unity Plugin

A Unity plugin for Yandex Games integration with minimal usage pattern. Provides auto-initialization, player data retrieval, cloud storage, and advertisement functionality using UniTask for async operations.

## Features

- **Zero Configuration**: Automatically initializes when your game starts
- **Player Data**: Get player information (name, avatar, language, device type)
- **Cloud Storage**: Save and load game data to Yandex Games cloud
- **Advertisements**: Show interstitial and rewarded video ads
- **UniTask Integration**: All async operations use UniTask for better performance
- **WebGL Support**: Designed specifically for Unity WebGL builds
- **Editor Testing**: Mock implementations for testing in Unity Editor

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
- `YandexGames.YandexGames.SaveDataAsync(key, data)` - Save data to cloud storage
- `YandexGames.YandexGames.LoadDataAsync(key)` - Load data from cloud storage
- `YandexGames.YandexGames.ShowInterstitialAdAsync()` - Show interstitial advertisement
- `YandexGames.YandexGames.ShowRewardedAdAsync(callback)` - Show rewarded video advertisement

**Note**: The static class `YandexGames` is inside the `YandexGames` namespace, so you need to use `YandexGames.YandexGames` to access the methods.

### Data Types

```csharp
public class PlayerData
{
    public string name;    // Player's display name
    public string avatar;  // Avatar URL
    public string lang;    // Language code (e.g., "en", "ru")
    public string device;  // Device type: "desktop", "mobile", "tablet"
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
2. **Auto-initialization**: Plugin automatically initializes when the game starts
3. **Callback Receiver**: A hidden GameObject (`YandexGamesCallbackReceiver`) handles JavaScript-to-C# callbacks
4. **JavaScript Bridge**: `.jslib` files handle Unity WebGL communication with Yandex Games SDK
5. **Async Pattern**: All operations use UniTask for high-performance async operations

The messaging flow:
- C# calls JavaScript functions via `DllImport`
- JavaScript calls Yandex Games SDK
- JavaScript uses `SendMessage` to call back to C# via the callback receiver GameObject
- Callback receiver forwards calls to the static class methods

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
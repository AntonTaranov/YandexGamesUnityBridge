# YandexGames Sample - Basic Usage

This sample demonstrates the minimal usage pattern for the YandexGames Unity plugin.

## Usage

1. Add the `YandexGamesExample` component to any GameObject in your scene
2. The plugin will automatically initialize when the game starts
3. Player data will be retrieved automatically (if `autoGetPlayerData` is enabled)
4. Cloud storage will be tested automatically (if `testCloudStorage` is enabled)

## Features Demonstrated

- **Auto-initialization**: Plugin initializes automatically when the game starts
- **Player Data**: Retrieve player information (name, avatar, language, device)
- **Cloud Storage**: Save and load data to/from Yandex Games cloud
- **Advertisements**: Show interstitial and rewarded ads

## API Examples

```csharp
// Get player data
var playerData = await YandexGames.GetPlayerDataAsync();

// Save data to cloud
await YandexGames.SaveDataAsync("saveKey", jsonData);

// Load data from cloud
var data = await YandexGames.LoadDataAsync("saveKey");

// Show interstitial ad
await YandexGames.ShowInterstitialAdAsync();

// Show rewarded ad
await YandexGames.ShowRewardedAdAsync(rewarded => {
    if (rewarded) {
        // Give reward to player
    }
});
```

## Requirements

- Unity 2022.3 LTS or newer
- UniTask package (automatically included as dependency)
- WebGL platform target
- Yandex Games SDK included in your HTML template
# Yandex Games Leaderboards & Remote Config

This document describes the Leaderboards and Remote Configuration features added to the Yandex Games Unity plugin.

## 📋 Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Leaderboards](#leaderboards)
  - [Submit Score](#submit-score)
  - [Get Player Entry](#get-player-entry)
  - [Get Leaderboard Entries](#get-leaderboard-entries)
  - [Get Leaderboard Description](#get-leaderboard-description)
- [Remote Configuration](#remote-configuration)
  - [Get Feature Flags](#get-feature-flags)
- [Data Models](#data-models)
- [Error Handling](#error-handling)
- [Examples](#examples)

## ✨ Features

### Leaderboards
- ✅ Submit player scores with optional metadata
- ✅ Retrieve current player's rank and score
- ✅ Fetch top players or players around current player
- ✅ Get leaderboard metadata (title, score format, sort order)
- ✅ Full UniTask async/await support
- ✅ WebGL-only with editor mock mode for testing

### Remote Configuration
- ✅ Fetch feature flags from Yandex remote config
- ✅ Support for default values (fallback)
- ✅ Client feature targeting (up to 10 features)
- ✅ Dictionary-based API for easy flag access

## 📦 Installation

The features are included in the `com.yandex.games` package. No additional installation required.

## 🏆 Leaderboards

All leaderboard methods require the Yandex Games SDK to be initialized. Check `YandexGames.IsInitialized` before calling.

### Submit Score

```csharp
using YandexGames;
using Cysharp.Threading.Tasks;

public async void SubmitScore()
{
    try
    {
        await YandexGames.SetLeaderboardScoreAsync("MyLeaderboard", 1000);
        Debug.Log("Score submitted!");
    }
    catch (YandexGamesException ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }
}
```

**Parameters:**
- `leaderboardName` (string): Technical name of the leaderboard (as configured in Yandex console)
- `score` (int): Non-negative integer score value
- `extraData` (string, optional): Additional data (max 128 characters)

**Notes:**
- For time-based leaderboards, submit time in milliseconds
- Rate limit: 1 request per second per player
- Automatic handling of decimal offsets based on leaderboard configuration

### Get Player Entry

Retrieve the current player's entry from a leaderboard:

```csharp
public async void GetMyRank()
{
    try
    {
        var entry = await YandexGames.GetLeaderboardPlayerEntryAsync("MyLeaderboard");
        Debug.Log($"Rank: {entry.rank + 1}"); // rank is 0-based
        Debug.Log($"Score: {entry.formattedScore}");
        Debug.Log($"Player: {entry.player.publicName}");
    }
    catch (YandexGamesException ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }
}
```

**Returns:** `LeaderboardEntry` with rank, score, player info, and formatted score

### Get Leaderboard Entries

Fetch multiple entries with flexible options:

```csharp
public async void ShowTop10()
{
    try
    {
        var response = await YandexGames.GetLeaderboardEntriesAsync(
            "MyLeaderboard",
            includeUser: true,      // Include current player
            quantityTop: 10         // Top 10 players
        );
        
        foreach (var entry in response.entries)
        {
            Debug.Log($"#{entry.rank + 1}: {entry.player.publicName} - {entry.formattedScore}");
        }
    }
    catch (YandexGamesException ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }
}
```

**Parameters:**
- `leaderboardName` (string): Leaderboard technical name
- `includeUser` (bool): Include current player in results
- `quantityAround` (int): Number of entries around player (requires `includeUser=true`)
- `quantityTop` (int): Number of top entries

**Returns:** `LeaderboardEntriesResponse` with:
- `leaderboard`: Leaderboard metadata
- `userRank`: Current player's rank (0-based)
- `ranges`: Array of entry ranges
- `entries`: Array of leaderboard entries

### Get Leaderboard Description

Retrieve leaderboard configuration and metadata:

```csharp
public async void GetLeaderboardInfo()
{
    try
    {
        var desc = await YandexGames.GetLeaderboardDescriptionAsync("MyLeaderboard");
        Debug.Log($"Title: {desc.title.en}");
        Debug.Log($"Type: {desc.description.score_format.type}"); // "numeric" or "time"
        Debug.Log($"Decimal Offset: {desc.description.score_format.options.decimal_offset}");
    }
    catch (YandexGamesException ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }
}
```

**Returns:** `LeaderboardDescription` with titles, score format, and sort order

## 🎛️ Remote Configuration

### Get Feature Flags

Fetch remote configuration flags with optional defaults and client targeting:

```csharp
using System.Collections.Generic;
using YandexGames.RemoteConfig;

public async void LoadFlags()
{
    try
    {
        // Define default values (used if remote fetch fails)
        var defaults = new Dictionary<string, string>
        {
            { "newFeature", "false" },
            { "difficulty", "normal" }
        };
        
        // Optional: Client features for targeting
        var clientFeatures = new ClientFeature[]
        {
            new ClientFeature("playerLevel", "10"),
            new ClientFeature("payingUser", "yes")
        };
        
        var flags = await YandexGames.GetFlagsAsync(defaults, clientFeatures);
        
        // Use flags
        if (flags["newFeature"] == "true")
        {
            // Enable new feature
        }
    }
    catch (YandexGamesException ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }
}
```

**Parameters:**
- `defaultFlags` (Dictionary<string, string>): Default flag values (fallback)
- `clientFeatures` (ClientFeature[], optional): Player context for targeting (max 10)

**Returns:** Dictionary of flag names to values

**Common Use Cases:**
- Feature toggles (enable/disable features remotely)
- A/B testing variants
- Dynamic configuration (difficulty, pricing)
- Phased rollouts

## 📊 Data Models

### Leaderboards Namespace

Located in `YandexGames.Leaderboards`:

- **`LeaderboardEntry`**: Single player entry (score, rank, player, extraData)
- **`LeaderboardPlayer`**: Player information (uniqueID, publicName, avatar permissions)
- **`ScopePermissions`**: Avatar and name display permissions
- **`LeaderboardDescription`**: Metadata (titles, score format, sort order)
- **`LocalizedTitles`**: Multi-language titles (en, ru, tr, de, fr, es, pt)
- **`DescriptionConfig`**: Score format and sort configuration
- **`ScoreFormat`**: Score type and options
- **`ScoreFormatOptions`**: Decimal offset for numeric scores
- **`LeaderboardEntriesResponse`**: Full response with entries and metadata
- **`EntryRange`**: Range specification (start, size)

### Remote Config Namespace

Located in `YandexGames.RemoteConfig`:

- **`ClientFeature`**: Player context parameter (name, value)

All classes are `[Serializable]` for Unity's JsonUtility compatibility.

## ⚠️ Error Handling

All methods throw `YandexGamesException` on failure. Common error scenarios:

```csharp
try
{
    await YandexGames.SetLeaderboardScoreAsync("MyLeaderboard", score);
}
catch (YandexGamesException ex)
{
    // Handle specific errors
    if (ex.Message.Contains("not initialized"))
    {
        Debug.LogError("SDK not initialized yet");
    }
    else if (ex.Message.Contains("rate limit"))
    {
        Debug.LogWarning("Slow down - rate limited");
    }
    else
    {
        Debug.LogError($"Unexpected error: {ex.Message}");
    }
}
```

**Common Errors:**
- "Plugin not initialized" - Wait for `YandexGames.IsInitialized`
- "Leaderboard name cannot be null or empty" - Invalid parameter
- "Score must be non-negative" - Invalid score value
- "Maximum 10 client features allowed" - Too many client features
- Network/Yandex SDK errors - Check console for details

## 📚 Examples

See the `Samples~` folder for complete examples:
- **`LeaderboardExample.cs`**: Score submission, rank retrieval, top players display
- **`RemoteConfigExample.cs`**: Feature flags, A/B testing, dynamic configuration

## 🔧 Platform Support

- **WebGL**: Full support (requires Yandex Games SDK in HTML)
- **Unity Editor**: Mock mode with fake data (for development/testing)
- **Other Platforms**: Not supported (methods will throw exception)

## 📖 API Reference

### YandexGames Static Class

```csharp
// Leaderboards
public static async UniTask SetLeaderboardScoreAsync(string leaderboardName, int score, string extraData = "")
public static async UniTask<LeaderboardDescription> GetLeaderboardDescriptionAsync(string leaderboardName)
public static async UniTask<LeaderboardEntry> GetLeaderboardPlayerEntryAsync(string leaderboardName)
public static async UniTask<LeaderboardEntriesResponse> GetLeaderboardEntriesAsync(
    string leaderboardName,
    bool includeUser = false,
    int quantityAround = 0,
    int quantityTop = 0)

// Remote Config
public static async UniTask<Dictionary<string, string>> GetFlagsAsync(
    Dictionary<string, string> defaultFlags = null,
    ClientFeature[] clientFeatures = null)
```

## 🎯 Best Practices

1. **Always check initialization:**
   ```csharp
   if (!YandexGames.IsInitialized)
   {
       Debug.LogWarning("Waiting for SDK initialization...");
       return;
   }
   ```

2. **Use try-catch for all async calls:**
   ```csharp
   try { await YandexGames.SetLeaderboardScoreAsync(...); }
   catch (YandexGamesException ex) { /* handle */ }
   ```

3. **Respect rate limits:**
   - Score submissions: 1 request/second per player
   - Add delays between rapid submissions

4. **Provide default flags:**
   ```csharp
   var flags = await YandexGames.GetFlagsAsync(defaults); // Always have fallback
   ```

5. **Test in editor:**
   - Mock mode returns fake data
   - Test WebGL build on Yandex platform for real behavior

## 📄 License

Same license as the main `com.yandex.games` package.

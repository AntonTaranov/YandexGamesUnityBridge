# Yandex Games - Quick API Reference

## Initialization

```csharp
// Wait for SDK to be ready
await UniTask.WaitUntil(() => YandexGames.IsInitialized);
```

## Leaderboards

### Submit Score
```csharp
await YandexGames.SetLeaderboardScoreAsync("MyLeaderboard", 1000);
await YandexGames.SetLeaderboardScoreAsync("MyLeaderboard", 1000, "extra data");
```

### Get My Rank
```csharp
var entry = await YandexGames.GetLeaderboardPlayerEntryAsync("MyLeaderboard");
Debug.Log($"Rank: {entry.rank + 1}, Score: {entry.formattedScore}");
```

### Get Top 10
```csharp
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

### Get Players Around Me
```csharp
var response = await YandexGames.GetLeaderboardEntriesAsync(
    "MyLeaderboard",
    includeUser: true,
    quantityAround: 5  // 5 above + 5 below + me = 11 total
);
```

### Get Leaderboard Info
```csharp
var desc = await YandexGames.GetLeaderboardDescriptionAsync("MyLeaderboard");
Debug.Log($"Title: {desc.title.en}");
Debug.Log($"Type: {desc.description.score_format.type}"); // "numeric" or "time"
```

## Remote Config

### Get Feature Flags
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

### With Client Targeting
```csharp
var clientFeatures = new ClientFeature[]
{
    new ClientFeature("playerLevel", "10"),
    new ClientFeature("payingUser", "yes")
};

var flags = await YandexGames.GetFlagsAsync(defaults, clientFeatures);
```

## Error Handling

```csharp
try
{
    await YandexGames.SetLeaderboardScoreAsync("MyLeaderboard", score);
}
catch (YandexGamesException ex)
{
    Debug.LogError($"Error: {ex.Message}");
}
```

## Common Data Models

### LeaderboardEntry
```csharp
entry.score            // int: Raw score value
entry.rank             // int: 0-based rank (0 = 1st place)
entry.formattedScore   // string: Formatted with decimal offset
entry.extraData        // string: Custom metadata
entry.player           // LeaderboardPlayer: Player info
```

### LeaderboardPlayer
```csharp
player.uniqueID       // string: Yandex player ID
player.publicName     // string: Display name or "User Hidden"
player.lang           // string: "en", "ru", "tr", etc.
player.scopePermissions // ScopePermissions: Avatar/name permissions
```

### LeaderboardEntriesResponse
```csharp
response.leaderboard  // LeaderboardDescription: Metadata
response.userRank     // int: Current player's rank (0-based)
response.entries      // LeaderboardEntry[]: Array of entries
response.ranges       // EntryRange[]: Ranges returned
```

## Best Practices

1. **Always wait for initialization**
   ```csharp
   await UniTask.WaitUntil(() => YandexGames.IsInitialized);
   ```

2. **Always use try-catch**
   ```csharp
   try { await method(); }
   catch (YandexGamesException ex) { /* handle */ }
   ```

3. **Respect rate limits** - Score submissions: 1/sec

4. **Provide default flags** - Always have fallback values

5. **Test in WebGL** - Editor returns mock data

## Namespaces

```csharp
using YandexGames;                // Main API
using YandexGames.Leaderboards;   // Leaderboard data models
using YandexGames.RemoteConfig;   // Remote config data models
using Cysharp.Threading.Tasks;    // UniTask
```

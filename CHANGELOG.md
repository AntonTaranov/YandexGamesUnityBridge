# Changelog

All notable changes to the Yandex Games Unity Plugin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.0] - 2025-12-29

### Added
- **In-App Purchases API**: Full support for Yandex Games payment system
  - `YandexGames.Payments.GetCatalogAsync()`: Retrieve product catalog with prices and images
  - `YandexGames.Payments.PurchaseAsync(productId, developerPayload)`: Initiate purchase flow with optional custom data
  - `YandexGames.Payments.GetPurchasesAsync()`: Get unconsumed purchases (REQUIRED for Yandex moderation)
  - `YandexGames.Payments.ConsumePurchaseAsync(purchaseToken)`: Mark purchase as consumed
  - `YandexGames.Payments.HasPurchase(productId)`: Convenience method for checking permanent purchases
- **Payment Data Models**: 4 new serializable classes
  - `YandexProduct`: Product catalog entry with pricing, title, description, image URI
  - `YandexPurchase`: Purchase transaction with token, payload, and optional signature
  - `YandexPaymentOptions`: Configuration for signed mode (server-side validation)
  - `CurrencyIconSize`: Enum for currency icon display sizes
- **JavaScript Bridge**: Extended `YandexGames.jslib` with 4 payment functions
  - `GetCatalogAsyncJS()`: Lazy initialization of payments API, catalog retrieval
  - `PurchaseAsyncJS(productId, developerPayload)`: Purchase flow with UTF8ToString parameter handling
  - `GetPurchasesAsyncJS()`: Unconsumed purchase list retrieval
  - `ConsumePurchaseAsyncJS(purchaseToken)`: Purchase consumption
- **Editor Mock Data**: Testing support for offline development
  - `MockCatalog`: 3 sample products (gold100, gold500, disable_ads)
  - `MockPurchases`: In-memory purchase list for editor testing
- **Documentation**: Complete payment integration guide
  - `Documentation~/Payments.md`: API reference, best practices, troubleshooting
  - Quick start examples for catalog display, purchase handling, unconsumed purchase recovery
  - Permanent vs consumable purchase patterns

### Technical Details
- All payment operations use UniTask for consistency with existing APIs
- WebGL-only implementation (editor returns mock data for testing)
- Lazy initialization pattern for `window.yandexPayments` (optimizes performance)
- Full error handling with descriptive `YandexGamesException` messages
- Parameter validation (null/empty checks, product existence validation)
- Unconsumed purchase recovery required for Yandex moderation approval
- Signature support for server-side validation (optional, advanced feature)
- Currency icon CDN integration via `GetPriceCurrencyImage()` method

### Changed
- Extended `YandexGamesCallbackReceiver` with 8 new payment callback methods
- Added `Payments` property to main `YandexGames` class for singleton access
- Updated package description to include in-app purchases
- Added new keywords: "payments", "iap", "leaderboards" to package.json
- Bumped package version to 1.4.0

### Security
- Purchase signature field for HMAC-SHA256 validation (server-side)
- Developer payload support for custom transaction metadata
- Warning logs to remind developers to save player data before consuming purchases

## [1.2.0] - 2025-01-XX

### Added
- **Leaderboards API**: Full support for Yandex Games Leaderboards
  - `SetLeaderboardScoreAsync()`: Submit player scores with optional metadata
  - `GetLeaderboardPlayerEntryAsync()`: Retrieve current player's rank and score
  - `GetLeaderboardEntriesAsync()`: Fetch top players or players around current player with flexible options
  - `GetLeaderboardDescriptionAsync()`: Get leaderboard metadata (titles, score format, sort order)
- **Remote Configuration API**: Feature flags and A/B testing support
  - `GetFlagsAsync()`: Fetch remote config flags with default values and client feature targeting
- **Data Models**: 11 serializable C# classes for JSON deserialization
  - `LeaderboardEntry`, `LeaderboardPlayer`, `ScopePermissions` (leaderboard entries)
  - `LeaderboardDescription`, `LocalizedTitles`, `DescriptionConfig` (leaderboard metadata)
  - `ScoreFormat`, `ScoreFormatOptions` (score display configuration)
  - `LeaderboardEntriesResponse`, `EntryRange` (API responses)
  - `ClientFeature` (remote config targeting)
- **JavaScript Bridge**: Extended `YandexGames.jslib` with 5 new bridge functions
  - `SetLeaderboardScoreAsyncJS`, `GetLeaderboardDescriptionAsyncJS`
  - `GetLeaderboardPlayerEntryAsyncJS`, `GetLeaderboardEntriesAsyncJS`
  - `GetFlagsAsyncJS`
- **Examples**: Added sample scripts in `Samples~` folder
  - `LeaderboardExample.cs`: Score submission, rank retrieval, top players display
  - `RemoteConfigExample.cs`: Feature flags, A/B testing patterns
- **Documentation**: Comprehensive guide for new features
  - `Documentation~/LeaderboardsAndRemoteConfig.md`: Full API reference and examples

### Technical Details
- All async operations use UniTask for consistency
- Editor mock mode for development/testing (returns fake data)
- WebGL-only implementation (throws exception on other platforms)
- Parameter validation for all public API methods
- Full error handling with `YandexGamesException`
- Rate limit documentation (score submission: 1 req/sec per player)
- Support for decimal offsets and time-based leaderboards
- Multi-language title support (en, ru, tr, de, fr, es, pt)
- Client feature targeting (max 10 features) for remote config

### Changed
- Extended `YandexGamesCallbackReceiver` with 10 new callback methods
- Added 5 new UniTaskCompletionSource fields to `YandexGames` class
- Added 5 new DllImport declarations for JavaScript bridge

## [1.1.0] - 2025-12-25

### Fixed
- **CRITICAL**: Fixed initialization flag timing race condition where `IsInitialized` was set to `true` immediately instead of waiting for JavaScript SDK callback
- `IsInitialized` now accurately reflects when Yandex Games SDK is truly ready to use in WebGL builds
- Eliminated race conditions between initialization and API calls during game startup

### Added
- Internal callback handlers `OnInitialized()` and `OnInitializeError()` for proper initialization state management
- Idempotent initialization - multiple `Initialize()` calls no longer cause duplicate setup
- `_isInitializing` state flag to prevent concurrent initialization attempts
- Initialization timeout documentation (10 seconds)
- Comprehensive XML documentation for initialization behavior and error handling

### Changed
- **BREAKING (behavior)**: In WebGL builds, `IsInitialized` is now `false` until JavaScript SDK confirms ready state (previously was `true` immediately)
- Recommended usage pattern: `await UniTask.WaitUntil(() => YandexGames.IsInitialized)` before calling APIs
- Unity Editor initialization remains immediate (no behavior change)

### Technical Details
- Added `OnInitialized()` callback from JavaScript to C# when YaGames.init() promise resolves
- Added `OnInitializeError(string error)` callback for SDK loading failures
- Enhanced error logging with detailed messages from JavaScript SDK
- Updated README.md with initialization behavior documentation

### Migration Guide
If your code assumed immediate initialization, add a wait before calling APIs:
```csharp
// Before (might fail with race condition):
void Start() {
    var player = YandexGames.GetPlayerDataAsync();
}

// After (safe):
async void Start() {
    await UniTask.WaitUntil(() => YandexGames.IsInitialized);
    var player = await YandexGames.GetPlayerDataAsync();
}
```

## [1.0.0] - 2024-01-12

### Added
- Initial release of Yandex Games Unity Plugin
- Auto-initialization with `RuntimeInitializeOnLoadMethod`
- Player data retrieval with `GetPlayerDataAsync()`
- Cloud storage operations with `SaveDataAsync()` and `LoadDataAsync()`
- Advertisement system with `ShowInterstitialAdAsync()` and `ShowRewardedAdAsync()`
- Complete UniTask integration for all async operations
- WebGL platform support with JavaScript bridge
- Mock implementations for Unity Editor testing
- Comprehensive error handling with `YandexGamesException`
- Unity Package Manager support with proper assembly definitions
- Basic usage sample and documentation

### Technical Features
- Zero-configuration setup following minimal usage pattern
- Unity 2022.3 LTS compatibility
- UniTask dependency for high-performance async operations
- WebGL-specific .jslib implementation
- Proper Unity-JavaScript callback integration
- Null-safe error handling and logging

### Documentation
- Complete API reference in README.md
- Basic usage examples and quick start guide
- Sample project demonstrating all features
- Inline code documentation with XML comments
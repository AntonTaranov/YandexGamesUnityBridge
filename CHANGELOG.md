# Changelog

All notable changes to the Yandex Games Unity Plugin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
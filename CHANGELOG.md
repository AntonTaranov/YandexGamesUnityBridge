# Changelog

All notable changes to the Yandex Games Unity Plugin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
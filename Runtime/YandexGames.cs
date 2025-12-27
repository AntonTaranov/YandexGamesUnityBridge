using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YandexGames.Leaderboards;
using YandexGames.RemoteConfig;

namespace YandexGames
{
    /// <summary>
    /// Main plugin class for Yandex Games integration
    /// </summary>
    public static class YandexGames
    {
        private static bool _isInitialized = false;
        private static bool _isInitializing = false;
        private const float INIT_TIMEOUT_SECONDS = 10f;
        private static UniTaskCompletionSource<string> _playerDataTask;
        private static UniTaskCompletionSource _saveDataTask;
        private static UniTaskCompletionSource<string> _loadDataTask;
        private static UniTaskCompletionSource _interstitialAdTask;
        private static UniTaskCompletionSource<bool> _rewardedAdTask;
        
        // Leaderboard task completion sources
        private static UniTaskCompletionSource _setLeaderboardScoreTask;
        private static UniTaskCompletionSource<string> _getLeaderboardDescriptionTask;
        private static UniTaskCompletionSource<string> _getLeaderboardPlayerEntryTask;
        private static UniTaskCompletionSource<string> _getLeaderboardEntriesTask;
        
        // Remote Config task completion source
        private static UniTaskCompletionSource<string> _getFlagsTask;
        
        /// <summary>
        /// Gets whether the plugin is initialized and ready to use
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Initialize Yandex Games plugin (called automatically)
        /// </summary>
        /// <remarks>
        /// In WebGL builds, initialization is asynchronous. The IsInitialized flag will remain false
        /// until the JavaScript SDK confirms successful initialization via callback.
        /// If initialization fails or times out (10 seconds), an error will be logged.
        /// In Unity Editor, initialization is immediate for development/testing purposes.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (!Application.isPlaying || _isInitialized || _isInitializing)
                return;

            // Create callback receiver GameObject
            var callbackReceiver = new GameObject("YandexGamesCallbackReceiver");
            callbackReceiver.AddComponent<YandexGamesCallbackReceiver>();

#if UNITY_WEBGL && !UNITY_EDITOR
            _isInitializing = true;
            try
            {
                YandexGamesInitialize();
                Debug.Log("[YandexGames] Initializing... (10s timeout)");
            }
            catch (Exception ex)
            {
                _isInitializing = false;
                Debug.LogError($"[YandexGames] Failed to initialize: {ex.Message}");
            }
#else
            _isInitialized = true; // Allow usage in editor for testing
            _isInitializing = false;
            Debug.LogWarning("[YandexGames] Plugin only works on WebGL platform. Running in mock mode.");
#endif
        }

        /// <summary>
        /// Get player data from Yandex Games
        /// </summary>
        /// <returns>Player data containing name, avatar, language, and device info</returns>
        public static async UniTask<PlayerData> GetPlayerDataAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            try
            {
                _playerDataTask = new UniTaskCompletionSource<string>();
                GetPlayerDataAsyncJS();
                var json = await _playerDataTask.Task;
                var playerData = JsonUtility.FromJson<PlayerData>(json);
                return playerData;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get player data: {ex.Message}", ex);
            }
#else
            // Mock data for testing in editor
            await UniTask.Delay(100);
            return new PlayerData
            {
                name = "Test Player",
                avatar = "",
                lang = "en",
                device = "desktop"
            };
#endif
        }

        /// <summary>
        /// Save data to Yandex Games cloud storage
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="data">Data to save (JSON string)</param>
        public static async UniTask SaveDataAsync(string key, string data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            try
            {
                _saveDataTask = new UniTaskCompletionSource();
                SaveDataAsyncJS(key, data ?? "");
                await _saveDataTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to save data: {ex.Message}", ex);
            }
#else
            // Mock delay for testing in editor
            await UniTask.Delay(50);
            Debug.Log($"[YandexGames] Mock save: {key} = {data}");
#endif
        }

        /// <summary>
        /// Load data from Yandex Games cloud storage
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <returns>Saved data (JSON string) or null if not found</returns>
        public static async UniTask<string> LoadDataAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            try
            {
                _loadDataTask = new UniTaskCompletionSource<string>();
                LoadDataAsyncJS(key);
                return await _loadDataTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to load data: {ex.Message}", ex);
            }
#else
            // Mock delay for testing in editor
            await UniTask.Delay(50);
            Debug.Log($"[YandexGames] Mock load: {key}");
            return null;
#endif
        }

        /// <summary>
        /// Show interstitial advertisement
        /// </summary>
        public static async UniTask ShowInterstitialAdAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            try
            {
                _interstitialAdTask = new UniTaskCompletionSource();
                ShowInterstitialAdAsyncJS();
                await _interstitialAdTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to show interstitial ad: {ex.Message}", ex);
            }
#else
            // Mock delay for testing in editor
            await UniTask.Delay(1000);
            Debug.Log("[YandexGames] Mock interstitial ad shown");
#endif
        }

        /// <summary>
        /// Show rewarded advertisement
        /// </summary>
        /// <param name="onResult">Callback with reward result (true if rewarded)</param>
        public static async UniTask ShowRewardedAdAsync(Action<bool> onResult)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            try
            {
                _rewardedAdTask = new UniTaskCompletionSource<bool>();
                ShowRewardedAdAsyncJS();
                var rewarded = await _rewardedAdTask.Task;
                onResult?.Invoke(rewarded);
            }
            catch (Exception ex)
            {
                onResult?.Invoke(false);
                throw new YandexGamesException($"Failed to show rewarded ad: {ex.Message}", ex);
            }
#else
            // Mock delay for testing in editor
            await UniTask.Delay(1000);
            Debug.Log("[YandexGames] Mock rewarded ad shown");
            onResult?.Invoke(true);
#endif
        }

        #region Leaderboards

        /// <summary>
        /// Submit a score to the leaderboard
        /// </summary>
        /// <param name="leaderboardName">Technical name of the leaderboard</param>
        /// <param name="score">Score value (non-negative integer)</param>
        /// <param name="extraData">Optional additional data (max 128 chars)</param>
        public static async UniTask SetLeaderboardScoreAsync(string leaderboardName, int score, string extraData = "")
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            if (string.IsNullOrEmpty(leaderboardName))
                throw new ArgumentException("Leaderboard name cannot be null or empty", nameof(leaderboardName));

            if (score < 0)
                throw new ArgumentException("Score must be non-negative", nameof(score));

            try
            {
                _setLeaderboardScoreTask = new UniTaskCompletionSource();
                SetLeaderboardScoreAsyncJS(leaderboardName, score, extraData);
                await _setLeaderboardScoreTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to set leaderboard score: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            Debug.Log($"[YandexGames] Mock: Set score {score} on leaderboard '{leaderboardName}'");
#endif
        }

        /// <summary>
        /// Get leaderboard metadata and configuration
        /// </summary>
        /// <param name="leaderboardName">Technical name of the leaderboard</param>
        /// <returns>Leaderboard description with display settings</returns>
        public static async UniTask<LeaderboardDescription> GetLeaderboardDescriptionAsync(string leaderboardName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            if (string.IsNullOrEmpty(leaderboardName))
                throw new ArgumentException("Leaderboard name cannot be null or empty", nameof(leaderboardName));

            try
            {
                _getLeaderboardDescriptionTask = new UniTaskCompletionSource<string>();
                GetLeaderboardDescriptionAsyncJS(leaderboardName);
                var json = await _getLeaderboardDescriptionTask.Task;
                return JsonUtility.FromJson<LeaderboardDescription>(json);
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get leaderboard description: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            return new LeaderboardDescription
            {
                name = leaderboardName,
                appID = "mock",
                @default = true,
                title = new LocalizedTitles { en = "Test Leaderboard", ru = "Тестовая таблица" },
                description = new DescriptionConfig
                {
                    invert_sort_order = false,
                    score_format = new ScoreFormat { type = "numeric", options = new ScoreFormatOptions { decimal_offset = 0 } }
                }
            };
#endif
        }

        /// <summary>
        /// Get current player's entry from leaderboard
        /// </summary>
        /// <param name="leaderboardName">Technical name of the leaderboard</param>
        /// <returns>Player's leaderboard entry</returns>
        public static async UniTask<LeaderboardEntry> GetLeaderboardPlayerEntryAsync(string leaderboardName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            if (string.IsNullOrEmpty(leaderboardName))
                throw new ArgumentException("Leaderboard name cannot be null or empty", nameof(leaderboardName));

            try
            {
                _getLeaderboardPlayerEntryTask = new UniTaskCompletionSource<string>();
                GetLeaderboardPlayerEntryAsyncJS(leaderboardName);
                var json = await _getLeaderboardPlayerEntryTask.Task;
                return JsonUtility.FromJson<LeaderboardEntry>(json);
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get player leaderboard entry: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            return new LeaderboardEntry
            {
                score = 1000,
                rank = 0,
                formattedScore = "1000",
                extraData = "",
                player = new LeaderboardPlayer
                {
                    uniqueID = "test123",
                    publicName = "Test Player",
                    lang = "en",
                    scopePermissions = new ScopePermissions { avatar = "allow", public_name = "allow" }
                }
            };
#endif
        }

        /// <summary>
        /// Get leaderboard entries with flexible options
        /// </summary>
        /// <param name="leaderboardName">Technical name of the leaderboard</param>
        /// <param name="includeUser">Include current player in results</param>
        /// <param name="quantityAround">Number of entries around player (requires includeUser=true)</param>
        /// <param name="quantityTop">Number of top entries to retrieve</param>
        /// <returns>Leaderboard entries response with metadata</returns>
        public static async UniTask<LeaderboardEntriesResponse> GetLeaderboardEntriesAsync(
            string leaderboardName,
            bool includeUser = false,
            int quantityAround = 0,
            int quantityTop = 0)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            if (string.IsNullOrEmpty(leaderboardName))
                throw new ArgumentException("Leaderboard name cannot be null or empty", nameof(leaderboardName));

            try
            {
                // Build options JSON
                var options = "{";
                options += $"\"includeUser\":{(includeUser ? "true" : "false")}";
                if (quantityAround > 0)
                    options += $",\"quantityAround\":{quantityAround}";
                if (quantityTop > 0)
                    options += $",\"quantityTop\":{quantityTop}";
                options += "}";

                _getLeaderboardEntriesTask = new UniTaskCompletionSource<string>();
                GetLeaderboardEntriesAsyncJS(leaderboardName, options);
                var json = await _getLeaderboardEntriesTask.Task;
                return JsonUtility.FromJson<LeaderboardEntriesResponse>(json);
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get leaderboard entries: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            return new LeaderboardEntriesResponse
            {
                leaderboard = new LeaderboardDescription
                {
                    name = leaderboardName,
                    appID = "mock",
                    @default = true,
                    title = new LocalizedTitles { en = "Test Leaderboard", ru = "Тестовая таблица" },
                    description = new DescriptionConfig
                    {
                        invert_sort_order = false,
                        score_format = new ScoreFormat { type = "numeric", options = new ScoreFormatOptions { decimal_offset = 0 } }
                    }
                },
                userRank = 0,
                ranges = new[] { new EntryRange { start = 0, size = 1 } },
                entries = new[]
                {
                    new LeaderboardEntry
                    {
                        score = 1000,
                        rank = 0,
                        formattedScore = "1000",
                        extraData = "",
                        player = new LeaderboardPlayer
                        {
                            uniqueID = "test123",
                            publicName = "Test Player",
                            lang = "en",
                            scopePermissions = new ScopePermissions { avatar = "allow", public_name = "allow" }
                        }
                    }
                }
            };
#endif
        }

        #endregion

        #region Remote Config

        /// <summary>
        /// Get remote configuration flags with optional client features
        /// </summary>
        /// <param name="defaultFlags">Default flag values if remote fetch fails</param>
        /// <param name="clientFeatures">Optional client features for targeting (max 10)</param>
        /// <returns>Dictionary of flag names to values</returns>
        public static async UniTask<System.Collections.Generic.Dictionary<string, string>> GetFlagsAsync(
            System.Collections.Generic.Dictionary<string, string> defaultFlags = null,
            ClientFeature[] clientFeatures = null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            if (clientFeatures != null && clientFeatures.Length > 10)
                throw new ArgumentException("Maximum 10 client features allowed", nameof(clientFeatures));

            try
            {
                // Build options JSON
                var options = "{";
                if (defaultFlags != null && defaultFlags.Count > 0)
                {
                    options += "\"defaultFlags\":{";
                    var first = true;
                    foreach (var kvp in defaultFlags)
                    {
                        if (!first) options += ",";
                        options += $"\"{kvp.Key}\":\"{kvp.Value}\"";
                        first = false;
                    }
                    options += "}";
                }

                if (clientFeatures != null && clientFeatures.Length > 0)
                {
                    if (defaultFlags != null && defaultFlags.Count > 0)
                        options += ",";
                    options += "\"clientFeatures\":[";
                    for (int i = 0; i < clientFeatures.Length; i++)
                    {
                        if (i > 0) options += ",";
                        options += $"{{\"name\":\"{clientFeatures[i].name}\",\"value\":\"{clientFeatures[i].value}\"}}";
                    }
                    options += "]";
                }
                options += "}";

                _getFlagsTask = new UniTaskCompletionSource<string>();
                GetFlagsAsyncJS(options);
                var json = await _getFlagsTask.Task;
                
                // Parse JSON response into dictionary
                var flags = new System.Collections.Generic.Dictionary<string, string>();
                // Simple JSON parsing for flat key-value pairs
                if (!string.IsNullOrEmpty(json) && json.Length > 2)
                {
                    json = json.Trim('{', '}');
                    var pairs = json.Split(',');
                    foreach (var pair in pairs)
                    {
                        var parts = pair.Split(':');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().Trim('"');
                            var value = parts[1].Trim().Trim('"');
                            flags[key] = value;
                        }
                    }
                }
                return flags;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get flags: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            return defaultFlags ?? new System.Collections.Generic.Dictionary<string, string>();
#endif
        }

        #endregion

        // Callback methods called from JavaScript
        /// <summary>
        /// Internal callback handler for successful initialization
        /// </summary>
        /// <remarks>
        /// Called by JavaScript after YaGames.init() promise resolves successfully.
        /// Sets IsInitialized flag to true and releases initialization state.
        /// </remarks>
        internal static void OnInitialized()
        {
            _isInitialized = true;
            _isInitializing = false;
            Debug.Log("[YandexGames] Plugin initialized successfully");
        }

        /// <summary>
        /// Internal callback handler for initialization errors
        /// </summary>
        /// <param name="error">Error message from JavaScript SDK</param>
        /// <remarks>
        /// Called by JavaScript if YaGames.init() fails or SDK is not found.
        /// This may also be triggered by timeout if no callback is received within 10 seconds.
        /// Logs error and keeps IsInitialized flag false.
        /// </remarks>
        internal static void OnInitializeError(string error)
        {
            _isInitializing = false;
            Debug.LogError($"[YandexGames] Failed to initialize: {error}");
        }

        public static void OnPlayerDataReceived(string json)
        {
            _playerDataTask?.TrySetResult(json);
        }

        public static void OnPlayerDataError(string error)
        {
            _playerDataTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnSaveDataComplete()
        {
            _saveDataTask?.TrySetResult();
        }

        public static void OnSaveDataError(string error)
        {
            _saveDataTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnLoadDataComplete(string data)
        {
            _loadDataTask?.TrySetResult(data);
        }

        public static void OnLoadDataError(string error)
        {
            _loadDataTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnInterstitialAdComplete()
        {
            _interstitialAdTask?.TrySetResult();
        }

        public static void OnInterstitialAdError(string error)
        {
            _interstitialAdTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnRewardedAdComplete(string rewardedStr)
        {
            var rewarded = rewardedStr == "true";
            _rewardedAdTask?.TrySetResult(rewarded);
        }

        public static void OnRewardedAdError(string error)
        {
            _rewardedAdTask?.TrySetException(new YandexGamesException(error));
        }

        // Leaderboard callbacks
        public static void OnSetLeaderboardScoreComplete()
        {
            _setLeaderboardScoreTask?.TrySetResult();
        }

        public static void OnSetLeaderboardScoreError(string error)
        {
            _setLeaderboardScoreTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnGetLeaderboardDescriptionComplete(string json)
        {
            _getLeaderboardDescriptionTask?.TrySetResult(json);
        }

        public static void OnGetLeaderboardDescriptionError(string error)
        {
            _getLeaderboardDescriptionTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnGetLeaderboardPlayerEntryComplete(string json)
        {
            _getLeaderboardPlayerEntryTask?.TrySetResult(json);
        }

        public static void OnGetLeaderboardPlayerEntryError(string error)
        {
            _getLeaderboardPlayerEntryTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnGetLeaderboardEntriesComplete(string json)
        {
            _getLeaderboardEntriesTask?.TrySetResult(json);
        }

        public static void OnGetLeaderboardEntriesError(string error)
        {
            _getLeaderboardEntriesTask?.TrySetException(new YandexGamesException(error));
        }

        // Remote Config callbacks
        public static void OnGetFlagsComplete(string json)
        {
            _getFlagsTask?.TrySetResult(json);
        }

        public static void OnGetFlagsError(string error)
        {
            _getFlagsTask?.TrySetException(new YandexGamesException(error));
        }

        // JavaScript bridge declarations
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void YandexGamesInitialize();

        [DllImport("__Internal")]
        private static extern void GetPlayerDataAsyncJS();

        [DllImport("__Internal")]
        private static extern void SaveDataAsyncJS(string key, string data);

        [DllImport("__Internal")]
        private static extern void LoadDataAsyncJS(string key);

        [DllImport("__Internal")]
        private static extern void ShowInterstitialAdAsyncJS();

        [DllImport("__Internal")]
        private static extern void ShowRewardedAdAsyncJS();

        // Leaderboard bridge declarations
        [DllImport("__Internal")]
        private static extern void SetLeaderboardScoreAsyncJS(string leaderboardName, int score, string extraData);

        [DllImport("__Internal")]
        private static extern void GetLeaderboardDescriptionAsyncJS(string leaderboardName);

        [DllImport("__Internal")]
        private static extern void GetLeaderboardPlayerEntryAsyncJS(string leaderboardName);

        [DllImport("__Internal")]
        private static extern void GetLeaderboardEntriesAsyncJS(string leaderboardName, string optionsJson);

        // Remote Config bridge declarations
        [DllImport("__Internal")]
        private static extern void GetFlagsAsyncJS(string optionsJson);
#endif
    }
}
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
        
        // Use dictionaries to track multiple concurrent calls with different parameters
        private static UniTaskCompletionSource<string> _playerDataTask;
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource> _saveDataTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource>();
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>> _loadDataTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>>();
        private static UniTaskCompletionSource _interstitialAdTask;
        private static UniTaskCompletionSource<bool> _rewardedAdTask;
        
        // Leaderboard task completion sources
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource> _setLeaderboardScoreTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource>();
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>> _getLeaderboardDescriptionTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>>();
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>> _getLeaderboardPlayerEntryTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>>();
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>> _getLeaderboardEntriesTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>>();
        
        // Remote Config task completion source
        private static readonly System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>> _getFlagsTasks = new System.Collections.Generic.Dictionary<string, UniTaskCompletionSource<string>>();
        
        // Review task completion sources (single calls per session)
        private static UniTaskCompletionSource<string> _canReviewTask;
        private static UniTaskCompletionSource<string> _requestReviewTask;
        
        /// <summary>
        /// Gets whether the plugin is initialized and ready to use
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Access to Yandex Games payment APIs
        /// </summary>
        public static YandexGamesPayments Payments => YandexGamesPayments.Instance;

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

            // Return existing task if already in progress
            if (_playerDataTask != null && _playerDataTask.Task.Status == UniTaskStatus.Pending)
                return _playerDataTask.Task.ContinueWith(json => JsonUtility.FromJson<PlayerData>(json));

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

            // Return existing task if already in progress for this key
            if (_saveDataTasks.TryGetValue(key, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
                return existingTask.Task;

            try
            {
                var task = new UniTaskCompletionSource();
                _saveDataTasks[key] = task;
                SaveDataAsyncJS(key, data ?? "");
                await task.Task;
                _saveDataTasks.Remove(key);
            }
            catch (Exception ex)
            {
                _saveDataTasks.Remove(key);
                throw new YandexGamesException($"Failed to save data: {ex.Message}", ex);
            }
#else
            // Mock mode: save to PlayerPrefs for testing in editor
            await UniTask.Delay(50);
            PlayerPrefs.SetString($"YandexGames_{key}", data ?? "");
            PlayerPrefs.Save();
            Debug.Log($"[YandexGames] Mock save to PlayerPrefs: {key}");
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

            // Return existing task if already in progress for this key
            if (_loadDataTasks.TryGetValue(key, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
                return existingTask.Task;

            try
            {
                var task = new UniTaskCompletionSource<string>();
                _loadDataTasks[key] = task;
                LoadDataAsyncJS(key);
                var result = await task.Task;
                _loadDataTasks.Remove(key);
                return result;
            }
            catch (Exception ex)
            {
                _loadDataTasks.Remove(key);
                throw new YandexGamesException($"Failed to load data: {ex.Message}", ex);
            }
#else
            // Mock mode: load from PlayerPrefs for testing in editor
            await UniTask.Delay(50);
            var value = PlayerPrefs.GetString($"YandexGames_{key}", null);
            Debug.Log($"[YandexGames] Mock load from PlayerPrefs: {key} = {(value != null ? "found" : "not found")}");
            return value;
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

            // Return existing task if already in progress
            if (_interstitialAdTask != null && _interstitialAdTask.Task.Status == UniTaskStatus.Pending)
                return _interstitialAdTask.Task;

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

            // Return existing task if already in progress
            if (_rewardedAdTask != null && _rewardedAdTask.Task.Status == UniTaskStatus.Pending)
            {
                return _rewardedAdTask.Task.ContinueWith(rewarded => { onResult?.Invoke(rewarded); });
            }

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

            // Return existing task if already in progress for this leaderboard
            if (_setLeaderboardScoreTasks.TryGetValue(leaderboardName, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
                return existingTask.Task;

            try
            {
                var task = new UniTaskCompletionSource();
                _setLeaderboardScoreTasks[leaderboardName] = task;
                SetLeaderboardScoreAsyncJS(leaderboardName, score, extraData);
                await task.Task;
                _setLeaderboardScoreTasks.Remove(leaderboardName);
            }
            catch (Exception ex)
            {
                _setLeaderboardScoreTasks.Remove(leaderboardName);
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

            // Return existing task if already in progress for this leaderboard
            if (_getLeaderboardDescriptionTasks.TryGetValue(leaderboardName, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
                return existingTask.Task.ContinueWith(json => JsonUtility.FromJson<LeaderboardDescription>(json));

            try
            {
                var task = new UniTaskCompletionSource<string>();
                _getLeaderboardDescriptionTasks[leaderboardName] = task;
                GetLeaderboardDescriptionAsyncJS(leaderboardName);
                var json = await task.Task;
                _getLeaderboardDescriptionTasks.Remove(leaderboardName);
                return JsonUtility.FromJson<LeaderboardDescription>(json);
            }
            catch (Exception ex)
            {
                _getLeaderboardDescriptionTasks.Remove(leaderboardName);
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

            // Return existing task if already in progress for this leaderboard
            if (_getLeaderboardPlayerEntryTasks.TryGetValue(leaderboardName, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
                return existingTask.Task.ContinueWith(json => JsonUtility.FromJson<LeaderboardEntry>(json));

            try
            {
                var task = new UniTaskCompletionSource<string>();
                _getLeaderboardPlayerEntryTasks[leaderboardName] = task;
                GetLeaderboardPlayerEntryAsyncJS(leaderboardName);
                var json = await task.Task;
                _getLeaderboardPlayerEntryTasks.Remove(leaderboardName);
                return JsonUtility.FromJson<LeaderboardEntry>(json);
            }
            catch (Exception ex)
            {
                _getLeaderboardPlayerEntryTasks.Remove(leaderboardName);
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

            // Build options JSON
            var options = "{";
            options += $"\"includeUser\":{(includeUser ? "true" : "false")}";
            if (quantityAround > 0)
                options += $",\"quantityAround\":{quantityAround}";
            if (quantityTop > 0)
                options += $",\"quantityTop\":{quantityTop}";
            options += "}";

            var requestKey = $"{leaderboardName}_{options}";

            // Return existing task if already in progress for this request
            if (_getLeaderboardEntriesTasks.TryGetValue(requestKey, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
                return existingTask.Task.ContinueWith(json => JsonUtility.FromJson<LeaderboardEntriesResponse>(json));

            try
            {
                var task = new UniTaskCompletionSource<string>();
                _getLeaderboardEntriesTasks[requestKey] = task;
                GetLeaderboardEntriesAsyncJS(leaderboardName, options);
                var json = await task.Task;
                _getLeaderboardEntriesTasks.Remove(requestKey);
                return JsonUtility.FromJson<LeaderboardEntriesResponse>(json);
            }
            catch (Exception ex)
            {
                _getLeaderboardEntriesTasks.Remove(requestKey);
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

            var requestKey = options;

            // Return existing task if already in progress for these options
            if (_getFlagsTasks.TryGetValue(requestKey, out var existingTask) && existingTask.Task.Status == UniTaskStatus.Pending)
            {
                return existingTask.Task.ContinueWith(json =>
                {
                    var flags = new System.Collections.Generic.Dictionary<string, string>();
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
                });
            }

            try
            {
                var task = new UniTaskCompletionSource<string>();
                _getFlagsTasks[requestKey] = task;
                GetFlagsAsyncJS(options);
                var json = await task.Task;
                _getFlagsTasks.Remove(requestKey);
                
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
                _getFlagsTasks.Remove(requestKey);
                throw new YandexGamesException($"Failed to get flags: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            return defaultFlags ?? new System.Collections.Generic.Dictionary<string, string>();
#endif
        }

        #endregion

        #region Review

        /// <summary>
        /// Check if review request is available for current user
        /// </summary>
        /// <returns>Tuple with availability flag and reason if unavailable</returns>
        public static async UniTask<(bool value, string reason)> CanReviewAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            // Return existing task if already in progress
            if (_canReviewTask != null && _canReviewTask.Task.Status == UniTaskStatus.Pending)
            {
                return _canReviewTask.Task.ContinueWith(json =>
                {
                    var value = json.Contains("\"value\":true");
                    var reason = "";
                    if (!value && json.Contains("\"reason\":"))
                    {
                        var reasonStart = json.IndexOf("\"reason\":\"") + 10;
                        var reasonEnd = json.IndexOf("\"", reasonStart);
                        if (reasonEnd > reasonStart)
                            reason = json.Substring(reasonStart, reasonEnd - reasonStart);
                    }
                    return (value, reason);
                });
            }

            try
            {
                _canReviewTask = new UniTaskCompletionSource<string>();
                CanReviewAsyncJS();
                var json = await _canReviewTask.Task;
                
                // Parse JSON: {"value":true} or {"value":false,"reason":"NO_AUTH"}
                var value = json.Contains("\"value\":true");
                var reason = "";
                
                if (!value && json.Contains("\"reason\":"))
                {
                    var reasonStart = json.IndexOf("\"reason\":\"") + 10;
                    var reasonEnd = json.IndexOf("\"", reasonStart);
                    if (reasonEnd > reasonStart)
                        reason = json.Substring(reasonStart, reasonEnd - reasonStart);
                }
                
                return (value, reason);
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to check review availability: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            Debug.Log("[YandexGames] Mock: Can review = true");
            return (true, "");
#endif
        }

        /// <summary>
        /// Request user to review the game
        /// </summary>
        /// <returns>True if user submitted feedback, false if closed popup</returns>
        /// <remarks>
        /// Can only be called once per session. Always use CanReviewAsync() before calling this method.
        /// </remarks>
        public static async UniTask<bool> RequestReviewAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_isInitialized)
                throw new YandexGamesException("Plugin not initialized");

            // Return existing task if already in progress
            if (_requestReviewTask != null && _requestReviewTask.Task.Status == UniTaskStatus.Pending)
            {
                return _requestReviewTask.Task.ContinueWith(json =>
                {
                    var feedbackSent = json.Contains("\"feedbackSent\":true");
                    return feedbackSent;
                });
            }

            try
            {
                _requestReviewTask = new UniTaskCompletionSource<string>();
                RequestReviewAsyncJS();
                var json = await _requestReviewTask.Task;
                
                // Parse JSON: {"feedbackSent":true} or {"feedbackSent":false}
                var feedbackSent = json.Contains("\"feedbackSent\":true");
                return feedbackSent;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to request review: {ex.Message}", ex);
            }
#else
            await UniTask.Delay(100);
            Debug.Log("[YandexGames] Mock: Review requested - feedback sent = true");
            return true;
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

        public static void OnSaveDataComplete(string key)
        {
            if (_saveDataTasks.TryGetValue(key, out var task))
                task?.TrySetResult();
        }

        public static void OnSaveDataError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<KeyErrorCallback>(json);
                if (_saveDataTasks.TryGetValue(callback.key, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _saveDataTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
        }

        public static void OnLoadDataComplete(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<KeyDataCallback>(json);
                if (_loadDataTasks.TryGetValue(callback.key, out var task))
                    task?.TrySetResult(callback.data);
            }
            catch
            {
                // Fallback for old format
                if (_loadDataTasks.Count == 1)
                {
                    foreach (var task in _loadDataTasks.Values)
                        task?.TrySetResult(json);
                }
            }
        }

        public static void OnLoadDataError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<KeyErrorCallback>(json);
                if (_loadDataTasks.TryGetValue(callback.key, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _loadDataTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
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
        public static void OnSetLeaderboardScoreComplete(string leaderboardName)
        {
            if (_setLeaderboardScoreTasks.TryGetValue(leaderboardName, out var task))
                task?.TrySetResult();
        }

        public static void OnSetLeaderboardScoreError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<LeaderboardErrorCallback>(json);
                if (_setLeaderboardScoreTasks.TryGetValue(callback.leaderboardName, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _setLeaderboardScoreTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
        }

        public static void OnGetLeaderboardDescriptionComplete(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<LeaderboardDataCallback>(json);
                if (_getLeaderboardDescriptionTasks.TryGetValue(callback.leaderboardName, out var task))
                    task?.TrySetResult(callback.data);
            }
            catch
            {
                // Fallback for old format
                if (_getLeaderboardDescriptionTasks.Count == 1)
                {
                    foreach (var task in _getLeaderboardDescriptionTasks.Values)
                        task?.TrySetResult(json);
                }
            }
        }

        public static void OnGetLeaderboardDescriptionError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<LeaderboardErrorCallback>(json);
                if (_getLeaderboardDescriptionTasks.TryGetValue(callback.leaderboardName, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _getLeaderboardDescriptionTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
        }

        public static void OnGetLeaderboardPlayerEntryComplete(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<LeaderboardDataCallback>(json);
                if (_getLeaderboardPlayerEntryTasks.TryGetValue(callback.leaderboardName, out var task))
                    task?.TrySetResult(callback.data);
            }
            catch
            {
                // Fallback for old format
                if (_getLeaderboardPlayerEntryTasks.Count == 1)
                {
                    foreach (var task in _getLeaderboardPlayerEntryTasks.Values)
                        task?.TrySetResult(json);
                }
            }
        }

        public static void OnGetLeaderboardPlayerEntryError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<LeaderboardErrorCallback>(json);
                if (_getLeaderboardPlayerEntryTasks.TryGetValue(callback.leaderboardName, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _getLeaderboardPlayerEntryTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
        }

        public static void OnGetLeaderboardEntriesComplete(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<RequestKeyDataCallback>(json);
                if (_getLeaderboardEntriesTasks.TryGetValue(callback.requestKey, out var task))
                    task?.TrySetResult(callback.data);
            }
            catch
            {
                // Fallback for old format
                if (_getLeaderboardEntriesTasks.Count == 1)
                {
                    foreach (var task in _getLeaderboardEntriesTasks.Values)
                        task?.TrySetResult(json);
                }
            }
        }

        public static void OnGetLeaderboardEntriesError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<RequestKeyErrorCallback>(json);
                if (_getLeaderboardEntriesTasks.TryGetValue(callback.requestKey, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _getLeaderboardEntriesTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
        }

        // Remote Config callbacks
        public static void OnGetFlagsComplete(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<RequestKeyDataCallback>(json);
                if (_getFlagsTasks.TryGetValue(callback.requestKey, out var task))
                    task?.TrySetResult(callback.data);
            }
            catch
            {
                // Fallback for old format
                if (_getFlagsTasks.Count == 1)
                {
                    foreach (var task in _getFlagsTasks.Values)
                        task?.TrySetResult(json);
                }
            }
        }

        public static void OnGetFlagsError(string json)
        {
            try
            {
                var callback = JsonUtility.FromJson<RequestKeyErrorCallback>(json);
                if (_getFlagsTasks.TryGetValue(callback.requestKey, out var task))
                    task?.TrySetException(new YandexGamesException(callback.error));
            }
            catch
            {
                // Fallback for old format
                foreach (var task in _getFlagsTasks.Values)
                    task?.TrySetException(new YandexGamesException(json));
            }
        }

        // Review callbacks
        public static void OnCanReviewComplete(string json)
        {
            _canReviewTask?.TrySetResult(json);
        }

        public static void OnCanReviewError(string error)
        {
            _canReviewTask?.TrySetException(new YandexGamesException(error));
        }

        public static void OnRequestReviewComplete(string json)
        {
            _requestReviewTask?.TrySetResult(json);
        }

        public static void OnRequestReviewError(string error)
        {
            _requestReviewTask?.TrySetException(new YandexGamesException(error));
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

        // Review bridge declarations
        [DllImport("__Internal")]
        private static extern void CanReviewAsyncJS();

        [DllImport("__Internal")]
        private static extern void RequestReviewAsyncJS();
#endif
    }
}
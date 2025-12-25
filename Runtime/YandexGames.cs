using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
#endif
    }
}
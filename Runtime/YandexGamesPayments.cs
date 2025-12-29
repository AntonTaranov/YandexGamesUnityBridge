using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YandexGames
{
    /// <summary>
    /// Manager class for Yandex Games payment APIs
    /// </summary>
    public class YandexGamesPayments
    {
        private static YandexGamesPayments _instance;
        
        // Task completion sources for async callbacks
        private static UniTaskCompletionSource<YandexProduct[]> _getCatalogTask;
        private static UniTaskCompletionSource<YandexPurchase> _purchaseTask;
        private static UniTaskCompletionSource<YandexPurchase[]> _getPurchasesTask;
        private static UniTaskCompletionSource _consumePurchaseTask;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static YandexGamesPayments Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new YandexGamesPayments();
                return _instance;
            }
        }

        private YandexGamesPayments() { }

        #region Mock Data for Editor Testing

        /// <summary>
        /// Mock product catalog for Unity Editor testing
        /// </summary>
        public static YandexProduct[] MockCatalog = new YandexProduct[]
        {
            new YandexProduct
            {
                id = "gold100",
                title = "100 Gold Coins",
                description = "A small pile of gold coins",
                imageURI = "https://example.com/gold100.png",
                price = "50 YAN",
                priceValue = "50",
                priceCurrencyCode = "YAN"
            },
            new YandexProduct
            {
                id = "gold500",
                title = "500 Gold Coins",
                description = "A large pile of gold coins",
                imageURI = "https://example.com/gold500.png",
                price = "200 YAN",
                priceValue = "200",
                priceCurrencyCode = "YAN"
            },
            new YandexProduct
            {
                id = "disable_ads",
                title = "Remove Ads",
                description = "Permanently remove all advertisements",
                imageURI = "https://example.com/noads.png",
                price = "500 YAN",
                priceValue = "500",
                priceCurrencyCode = "YAN"
            }
        };

        /// <summary>
        /// Mock purchase list for Unity Editor testing
        /// </summary>
        public static List<YandexPurchase> MockPurchases = new List<YandexPurchase>();

        #endregion

        #region Public API Methods

        /// <summary>
        /// Retrieves the product catalog from Yandex Games
        /// </summary>
        /// <returns>Array of available products with pricing information</returns>
        /// <exception cref="YandexGamesException">
        /// Thrown when SDK not initialized, network error, or catalog unavailable
        /// </exception>
        /// <remarks>
        /// Products are fetched fresh on each call to ensure up-to-date pricing.
        /// In Unity Editor, returns mock catalog for testing.
        /// </remarks>
        public async UniTask<YandexProduct[]> GetCatalogAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!YandexGames.IsInitialized)
                throw new YandexGamesException("Plugin not initialized");

            // Return existing task if already in progress
            if (_getCatalogTask != null && _getCatalogTask.Task.Status == UniTaskStatus.Pending)
                return _getCatalogTask.Task;

            try
            {
                _getCatalogTask = new UniTaskCompletionSource<YandexProduct[]>();
                GetCatalogAsyncJS();
                return await _getCatalogTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get catalog: {ex.Message}", ex);
            }
#else
            // Editor fallback - return mock catalog
            await UniTask.Delay(100);
            return MockCatalog;
#endif
        }

        /// <summary>
        /// Initiates purchase flow for specified product
        /// </summary>
        /// <param name="productId">Product ID from catalog (must match Yandex Console)</param>
        /// <param name="developerPayload">Optional custom data to attach to purchase (e.g., player ID, server ID)</param>
        /// <returns>Purchase object with product ID, token, and payload</returns>
        /// <exception cref="ArgumentException">Thrown when productId is null or empty</exception>
        /// <exception cref="YandexGamesException">
        /// Thrown when SDK not initialized, product not found, payment cancelled, 
        /// network error, or insufficient funds
        /// </exception>
        /// <remarks>
        /// Opens Yandex payment dialog. Returns when payment succeeds or throws when cancelled/failed.
        /// In Unity Editor, simulates purchase with mock data.
        /// 
        /// IMPORTANT: After successful purchase, grant items to player BEFORE consuming purchase token.
        /// If game crashes between purchase and consumption, use GetPurchasesAsync() on next startup
        /// to recover unconsumed purchases.
        /// </remarks>
        public async UniTask<YandexPurchase> PurchaseAsync(string productId, string developerPayload = null)
        {
            if (string.IsNullOrEmpty(productId))
                throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!YandexGames.IsInitialized)
                throw new YandexGamesException("Plugin not initialized");

            // Return existing task if already in progress
            if (_purchaseTask != null && _purchaseTask.Task.Status == UniTaskStatus.Pending)
                return _purchaseTask.Task;

            try
            {
                _purchaseTask = new UniTaskCompletionSource<YandexPurchase>();
                PurchaseAsyncJS(productId, developerPayload ?? "");
                var purchase = await _purchaseTask.Task;
                
                // Warn developer to save before consuming
                Debug.LogWarning("[YandexGames] Remember to grant items and save player data BEFORE consuming purchase!");
                
                return purchase;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to purchase product: {ex.Message}", ex);
            }
#else
            // Editor fallback - simulate purchase
            await UniTask.Delay(500); // Simulate payment dialog delay

            // Validate product exists in mock catalog
            if (!MockCatalog.Any(p => p.Id == productId))
                throw new YandexGamesException($"Product not found: {productId}");

            var purchase = new YandexPurchase
            {
                productID = productId,
                purchaseToken = $"mock_token_{Guid.NewGuid():N}",
                developerPayload = developerPayload ?? "",
                signature = ""
            };

            MockPurchases.Add(purchase);
            Debug.Log($"[YandexGames] Mock purchase created: {productId}");
            Debug.LogWarning("[YandexGames] Remember to grant items and save player data BEFORE consuming purchase!");
            
            return purchase;
#endif
        }

        /// <summary>
        /// Retrieves list of unconsumed purchases for current player
        /// </summary>
        /// <returns>Array of unconsumed purchases</returns>
        /// <exception cref="YandexGamesException">
        /// Thrown when SDK not initialized, player not authorized (USER_NOT_AUTHORIZED),
        /// or network error occurs
        /// </exception>
        /// <remarks>
        /// Call this on game startup to process purchases that were not consumed
        /// (due to crashes, network issues, etc.). This is REQUIRED for Yandex moderation.
        /// 
        /// For permanent purchases (like ad removal), check if product exists in list
        /// without consuming. For consumable purchases, grant items and consume.
        /// 
        /// Requires player to be authorized - unauthorized players will receive
        /// USER_NOT_AUTHORIZED error.
        /// 
        /// In Unity Editor, returns mock purchase list for testing.
        /// </remarks>
        public async UniTask<YandexPurchase[]> GetPurchasesAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!YandexGames.IsInitialized)
                throw new YandexGamesException("Plugin not initialized");

            // Return existing task if already in progress
            if (_getPurchasesTask != null && _getPurchasesTask.Task.Status == UniTaskStatus.Pending)
                return _getPurchasesTask.Task;

            try
            {
                _getPurchasesTask = new UniTaskCompletionSource<YandexPurchase[]>();
                GetPurchasesAsyncJS();
                return await _getPurchasesTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to get purchases: {ex.Message}", ex);
            }
#else
            // Editor fallback - return mock purchases
            await UniTask.Delay(100);
            return MockPurchases.ToArray();
#endif
        }

        /// <summary>
        /// Marks a consumable purchase as used and removes it from purchase list
        /// </summary>
        /// <param name="purchaseToken">Unique purchase token from Purchase object</param>
        /// <exception cref="ArgumentException">Thrown when purchaseToken is null or empty</exception>
        /// <exception cref="YandexGamesException">
        /// Thrown when SDK not initialized, token invalid/not found, 
        /// token already consumed, or network error occurs
        /// </exception>
        /// <remarks>
        /// WARNING: After consumption, purchase is permanently removed and cannot be recovered.
        /// Always grant items to player BEFORE calling this method.
        /// 
        /// For permanent purchases (ad removal, unlocks), do NOT consume - check via
        /// GetPurchasesAsync() on each game startup instead.
        /// 
        /// In Unity Editor, removes purchase from mock list.
        /// </remarks>
        public async UniTask ConsumePurchaseAsync(string purchaseToken)
        {
            if (string.IsNullOrEmpty(purchaseToken))
                throw new ArgumentException("Purchase token cannot be null or empty", nameof(purchaseToken));

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!YandexGames.IsInitialized)
                throw new YandexGamesException("Plugin not initialized");

            // Return existing task if already in progress
            if (_consumePurchaseTask != null && _consumePurchaseTask.Task.Status == UniTaskStatus.Pending)
                return _consumePurchaseTask.Task;

            try
            {
                _consumePurchaseTask = new UniTaskCompletionSource();
                ConsumePurchaseAsyncJS(purchaseToken);
                await _consumePurchaseTask.Task;
            }
            catch (Exception ex)
            {
                throw new YandexGamesException($"Failed to consume purchase: {ex.Message}", ex);
            }
#else
            // Editor fallback - remove from mock list
            var purchase = MockPurchases.FirstOrDefault(p => p.PurchaseToken == purchaseToken);
            if (purchase == null)
                throw new YandexGamesException($"Purchase token not found: {purchaseToken}");

            MockPurchases.Remove(purchase);
            Debug.Log($"[YandexGames] Mock purchase consumed: {purchase.ProductID}");
#endif
        }

        /// <summary>
        /// Checks if player has specific product in unconsumed purchases
        /// </summary>
        /// <param name="productId">Product ID to check</param>
        /// <returns>True if product exists in unconsumed purchases, false otherwise</returns>
        /// <remarks>
        /// Convenience method for checking permanent purchases like ad removal.
        /// Internally calls GetPurchasesAsync() and searches for product ID.
        /// 
        /// Returns false if player not authorized or any error occurs.
        /// </remarks>
        public async UniTask<bool> HasPurchase(string productId)
        {
            try
            {
                var purchases = await GetPurchasesAsync();
                return purchases.Any(p => p.ProductID == productId);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region JavaScript Bridge (WebGL)

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void GetCatalogAsyncJS();

        [DllImport("__Internal")]
        private static extern void PurchaseAsyncJS(string productId, string developerPayload);

        [DllImport("__Internal")]
        private static extern void GetPurchasesAsyncJS();

        [DllImport("__Internal")]
        private static extern void ConsumePurchaseAsyncJS(string purchaseToken);
#endif

        #endregion

        #region Internal Callbacks (called from JavaScript)

        internal static void OnGetCatalogComplete(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<ProductArrayWrapper>("{\"products\":" + json + "}");
                _getCatalogTask?.TrySetResult(wrapper.products);
            }
            catch (Exception ex)
            {
                _getCatalogTask?.TrySetException(new YandexGamesException($"Failed to parse catalog: {ex.Message}", ex));
            }
        }

        internal static void OnGetCatalogError(string error)
        {
            _getCatalogTask?.TrySetException(new YandexGamesException(error));
        }

        internal static void OnPurchaseComplete(string json)
        {
            try
            {
                var purchase = JsonUtility.FromJson<YandexPurchase>(json);
                _purchaseTask?.TrySetResult(purchase);
            }
            catch (Exception ex)
            {
                _purchaseTask?.TrySetException(new YandexGamesException($"Failed to parse purchase: {ex.Message}", ex));
            }
        }

        internal static void OnPurchaseError(string error)
        {
            _purchaseTask?.TrySetException(new YandexGamesException(error));
        }

        internal static void OnGetPurchasesComplete(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<PurchaseArrayWrapper>("{\"purchases\":" + json + "}");
                _getPurchasesTask?.TrySetResult(wrapper.purchases);
            }
            catch (Exception ex)
            {
                _getPurchasesTask?.TrySetException(new YandexGamesException($"Failed to parse purchases: {ex.Message}", ex));
            }
        }

        internal static void OnGetPurchasesError(string error)
        {
            _getPurchasesTask?.TrySetException(new YandexGamesException(error));
        }

        internal static void OnConsumePurchaseComplete(string empty)
        {
            _consumePurchaseTask?.TrySetResult();
        }

        internal static void OnConsumePurchaseError(string error)
        {
            _consumePurchaseTask?.TrySetException(new YandexGamesException(error));
        }

        #endregion

        #region Helper Classes for JSON Deserialization

        [Serializable]
        private class ProductArrayWrapper
        {
            public YandexProduct[] products;
        }

        [Serializable]
        private class PurchaseArrayWrapper
        {
            public YandexPurchase[] purchases;
        }

        #endregion
    }
}

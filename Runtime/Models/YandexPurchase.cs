using System;
using UnityEngine;

namespace YandexGames
{
    /// <summary>
    /// Represents a purchase made through Yandex Games payment system
    /// </summary>
    [Serializable]
    public class YandexPurchase
    {
        /// <summary>
        /// Product identifier from catalog
        /// </summary>
        public string productID;
        
        /// <summary>
        /// Unique purchase token for consumption
        /// </summary>
        public string purchaseToken;
        
        /// <summary>
        /// Custom developer payload attached during purchase
        /// </summary>
        public string developerPayload;
        
        /// <summary>
        /// Purchase signature for server-side validation (only in signed mode)
        /// </summary>
        /// <remarks>
        /// Empty in default mode. Only populated when payment options specify signed: true.
        /// Use for server-side validation via HMAC-SHA256 signature verification.
        /// </remarks>
        public string signature;

        // Property accessors
        public string ProductID => productID;
        public string PurchaseToken => purchaseToken;
        public string DeveloperPayload => developerPayload;
        public string Signature => signature;
    }
}

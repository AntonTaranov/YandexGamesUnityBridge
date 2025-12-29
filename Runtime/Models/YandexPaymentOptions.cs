using System;
using UnityEngine;

namespace YandexGames
{
    /// <summary>
    /// Payment initialization options (reserved for future use)
    /// </summary>
    [Serializable]
    public class YandexPaymentOptions
    {
        /// <summary>
        /// Enable signed mode for server-side purchase validation
        /// </summary>
        /// <remarks>
        /// When true, purchases include HMAC-SHA256 signature for server validation.
        /// Secret key must be configured in Yandex Console.
        /// </remarks>
        public bool signed = false;

        public bool Signed => signed;
    }
}

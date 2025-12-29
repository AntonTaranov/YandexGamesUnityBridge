using System;
using UnityEngine;

namespace YandexGames
{
    /// <summary>
    /// Represents a product available for purchase in Yandex Games catalog
    /// </summary>
    [Serializable]
    public class YandexProduct
    {
        /// <summary>
        /// Unique product identifier (matches ID in Yandex Console)
        /// </summary>
        public string id;
        
        /// <summary>
        /// Product title (localized)
        /// </summary>
        public string title;
        
        /// <summary>
        /// Product description (localized)
        /// </summary>
        public string description;
        
        /// <summary>
        /// Product image URL
        /// </summary>
        public string imageURI;
        
        /// <summary>
        /// Formatted price with currency symbol (e.g., "99.00 ₽")
        /// </summary>
        public string price;
        
        /// <summary>
        /// Numeric price value (e.g., "99.00")
        /// </summary>
        public string priceValue;
        
        /// <summary>
        /// Currency code (e.g., "RUB", "USD", "EUR")
        /// </summary>
        public string priceCurrencyCode;

        // Property accessors
        public string Id => id;
        public string Title => title;
        public string Description => description;
        public string ImageURI => imageURI;
        public string Price => price;
        public string PriceValue => priceValue;
        public string PriceCurrencyCode => priceCurrencyCode;

        /// <summary>
        /// Gets the URL for the currency icon from Yandex CDN
        /// </summary>
        /// <param name="size">Icon size (Small, Medium, or Svg)</param>
        /// <returns>Yandex CDN URL for currency icon</returns>
        public string GetPriceCurrencyImage(CurrencyIconSize size)
        {
            string sizeStr = size switch
            {
                CurrencyIconSize.Small => "small",
                CurrencyIconSize.Medium => "medium",
                CurrencyIconSize.Svg => "svg",
                _ => "medium"
            };
            
            return $"https://yastatic.net/s3/web-payment/trust/icons/{sizeStr}/{priceCurrencyCode}.png";
        }
    }
}

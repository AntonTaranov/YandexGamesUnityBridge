using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents player information within a leaderboard entry
    /// </summary>
    [Serializable]
    public class LeaderboardPlayer
    {
        /// <summary>
        /// Unique player identifier from Yandex platform
        /// </summary>
        public string uniqueID;
        
        /// <summary>
        /// Player's display name, or "User Hidden" if privacy settings restrict access
        /// </summary>
        public string publicName;
        
        /// <summary>
        /// Player's language preference (ISO 639-1 code: "ru", "en", "tr", etc.)
        /// </summary>
        public string lang;
        
        /// <summary>
        /// Scope permissions for avatar and public name access
        /// </summary>
        public ScopePermissions scopePermissions;
        
        /// <summary>
        /// Gets avatar URL for specified size
        /// </summary>
        /// <param name="size">Avatar size: "small", "medium", or "large"</param>
        /// <returns>Yandex CDN URL or empty string if permission denied</returns>
        public string GetAvatarSrc(string size)
        {
            if (scopePermissions?.avatar == "forbid")
                return string.Empty;
            
            // URL would be constructed from Yandex response in actual implementation
            // For now, return empty as URL is provided by Yandex SDK in JS
            return string.Empty;
        }
        
        /// <summary>
        /// Gets avatar srcset for Retina displays
        /// </summary>
        /// <param name="size">Avatar size: "small", "medium", or "large"</param>
        /// <returns>Srcset string for responsive images</returns>
        public string GetAvatarSrcSet(string size)
        {
            if (scopePermissions?.avatar == "forbid")
                return string.Empty;
            
            // URL would be constructed from Yandex response in actual implementation
            return string.Empty;
        }
    }
}

using System;

namespace YandexGames
{
    /// <summary>
    /// Player data from Yandex Games
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        /// <summary>
        /// Player's display name
        /// </summary>
        public string name;
        
        /// <summary>
        /// URL to player's avatar image
        /// </summary>
        public string avatar;
        
        /// <summary>
        /// Player's language code (e.g., "en", "ru")
        /// </summary>
        public string lang;
        
        /// <summary>
        /// Device type: "desktop", "mobile", or "tablet"
        /// </summary>
        public string device;
    }
}
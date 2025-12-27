using System;

namespace YandexGames.RemoteConfig
{
    /// <summary>
    /// Represents a player-specific parameter sent to remote config system
    /// </summary>
    [Serializable]
    public class ClientFeature
    {
        /// <summary>
        /// Feature parameter name (e.g., "payingStatus", "levels", "inAppPurchaseUsed")
        /// </summary>
        public string name;
        
        /// <summary>
        /// Feature parameter value (string representation, e.g., "5", "yes", "paying")
        /// </summary>
        public string value;
        
        /// <summary>
        /// Constructor for easy initialization
        /// </summary>
        /// <param name="name">Feature parameter name</param>
        /// <param name="value">Feature parameter value</param>
        public ClientFeature(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}

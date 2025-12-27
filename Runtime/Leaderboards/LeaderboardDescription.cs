using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents leaderboard metadata and configuration
    /// </summary>
    [Serializable]
    public class LeaderboardDescription
    {
        /// <summary>
        /// Application identifier
        /// </summary>
        public string appID;
        
        /// <summary>
        /// Technical name of the leaderboard (used in API calls)
        /// </summary>
        public string name;
        
        /// <summary>
        /// Whether this is the default leaderboard for the application
        /// </summary>
        public bool @default;
        
        /// <summary>
        /// Localized display titles
        /// </summary>
        public LocalizedTitles title;
        
        /// <summary>
        /// Score display and sorting configuration
        /// </summary>
        public DescriptionConfig description;
    }
}

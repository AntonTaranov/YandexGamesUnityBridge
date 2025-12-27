using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents the complete response from getEntries() request
    /// </summary>
    [Serializable]
    public class LeaderboardEntriesResponse
    {
        /// <summary>
        /// Description of the leaderboard
        /// </summary>
        public LeaderboardDescription leaderboard;
        
        /// <summary>
        /// Current player's rank (0-based), or 0 if not included in request
        /// </summary>
        public int userRank;
        
        /// <summary>
        /// Ranges of entries returned
        /// </summary>
        public EntryRange[] ranges;
        
        /// <summary>
        /// Array of leaderboard entries matching the request
        /// </summary>
        public LeaderboardEntry[] entries;
    }
}

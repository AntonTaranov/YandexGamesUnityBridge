using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents a single player's entry in a leaderboard
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        /// <summary>
        /// Player's score value (non-negative integer).
        /// For time-based leaderboards, value is in milliseconds.
        /// </summary>
        public int score;
        
        /// <summary>
        /// Optional additional player information
        /// </summary>
        public string extraData;
        
        /// <summary>
        /// Player's rank in leaderboard (0-based: rank 0 = 1st place)
        /// </summary>
        public int rank;
        
        /// <summary>
        /// Player information associated with this entry
        /// </summary>
        public LeaderboardPlayer player;
        
        /// <summary>
        /// Score formatted according to leaderboard settings (e.g., "12.34" for decimal offset 2)
        /// </summary>
        public string formattedScore;
    }
}

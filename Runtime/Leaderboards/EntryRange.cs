using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents a range of leaderboard entries
    /// </summary>
    [Serializable]
    public class EntryRange
    {
        /// <summary>
        /// Starting rank of this range (0-based)
        /// </summary>
        public int start;
        
        /// <summary>
        /// Number of entries requested in this range.
        /// May not match actual entries returned if insufficient data.
        /// </summary>
        public int size;
    }
}

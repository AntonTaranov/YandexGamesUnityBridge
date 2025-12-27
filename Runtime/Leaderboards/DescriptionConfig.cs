using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents leaderboard description configuration
    /// </summary>
    [Serializable]
    public class DescriptionConfig
    {
        /// <summary>
        /// Sort order: false = descending (higher is better), true = ascending (lower is better)
        /// </summary>
        public bool invert_sort_order;
        
        /// <summary>
        /// Score format specification
        /// </summary>
        public ScoreFormat score_format;
    }
}

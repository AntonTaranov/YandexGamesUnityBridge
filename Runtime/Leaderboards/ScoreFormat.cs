using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents score format specification
    /// </summary>
    [Serializable]
    public class ScoreFormat
    {
        /// <summary>
        /// Score type: "numeric" or "time"
        /// </summary>
        public string type;
        
        /// <summary>
        /// Format options (decimal offset for numeric scores)
        /// </summary>
        public ScoreFormatOptions options;
    }
}

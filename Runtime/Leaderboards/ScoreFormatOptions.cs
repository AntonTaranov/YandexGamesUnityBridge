using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents score format options
    /// </summary>
    [Serializable]
    public class ScoreFormatOptions
    {
        /// <summary>
        /// Number of decimal places to display (e.g., 2 means 1234 displays as "12.34")
        /// </summary>
        public int decimal_offset;
    }
}

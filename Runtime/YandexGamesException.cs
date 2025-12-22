using System;

namespace YandexGames
{
    /// <summary>
    /// Exception thrown by YandexGames plugin operations
    /// </summary>
    public class YandexGamesException : Exception
    {
        public YandexGamesException(string message) : base(message) { }
        public YandexGamesException(string message, Exception innerException) : base(message, innerException) { }
    }
}
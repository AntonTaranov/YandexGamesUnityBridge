using System;

namespace YandexGames
{
    /// <summary>
    /// Helper classes for parsing JavaScript callback data
    /// </summary>
    [Serializable]
    internal class KeyDataCallback
    {
        public string key;
        public string data;
    }

    [Serializable]
    internal class KeyErrorCallback
    {
        public string key;
        public string error;
    }

    [Serializable]
    internal class LeaderboardDataCallback
    {
        public string leaderboardName;
        public string data; // JSON string of actual data
    }

    [Serializable]
    internal class LeaderboardErrorCallback
    {
        public string leaderboardName;
        public string error;
    }

    [Serializable]
    internal class RequestKeyDataCallback
    {
        public string requestKey;
        public string data; // JSON string of actual data
    }

    [Serializable]
    internal class RequestKeyErrorCallback
    {
        public string requestKey;
        public string error;
    }
}

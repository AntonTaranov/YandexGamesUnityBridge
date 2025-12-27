using System;

namespace YandexGames.Leaderboards
{
    /// <summary>
    /// Represents player permission settings for avatar and name display
    /// </summary>
    [Serializable]
    public class ScopePermissions
    {
        /// <summary>
        /// Permission status for avatar access ("allow" or "forbid")
        /// </summary>
        public string avatar;
        
        /// <summary>
        /// Permission status for public name access ("allow" or "forbid")
        /// </summary>
        public string public_name;
    }
}

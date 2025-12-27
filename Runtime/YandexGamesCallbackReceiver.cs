using UnityEngine;

namespace YandexGames
{
    /// <summary>
    /// Internal callback receiver for YandexGames JavaScript messages
    /// </summary>
    internal class YandexGamesCallbackReceiver : MonoBehaviour
    {
        private void Awake()
        {
            // Ensure this object persists across scenes
            DontDestroyOnLoad(gameObject);
            gameObject.name = "YandexGamesCallbackReceiver";
        }

        // Callback methods called from JavaScript via SendMessage
        public void OnInitialized()
        {
            YandexGames.OnInitialized();
        }

        public void OnInitializeError(string error)
        {
            YandexGames.OnInitializeError(error);
        }

        public void OnPlayerDataReceived(string json)
        {
            YandexGames.OnPlayerDataReceived(json);
        }

        public void OnPlayerDataError(string error)
        {
            YandexGames.OnPlayerDataError(error);
        }

        public void OnSaveDataComplete()
        {
            YandexGames.OnSaveDataComplete();
        }

        public void OnSaveDataError(string error)
        {
            YandexGames.OnSaveDataError(error);
        }

        public void OnLoadDataComplete(string data)
        {
            YandexGames.OnLoadDataComplete(data);
        }

        public void OnLoadDataError(string error)
        {
            YandexGames.OnLoadDataError(error);
        }

        public void OnInterstitialAdComplete()
        {
            YandexGames.OnInterstitialAdComplete();
        }

        public void OnInterstitialAdError(string error)
        {
            YandexGames.OnInterstitialAdError(error);
        }

        public void OnRewardedAdComplete(string rewardedStr)
        {
            YandexGames.OnRewardedAdComplete(rewardedStr);
        }

        public void OnRewardedAdError(string error)
        {
            YandexGames.OnRewardedAdError(error);
        }

        // Leaderboards callbacks
        public void OnSetLeaderboardScoreComplete()
        {
            YandexGames.OnSetLeaderboardScoreComplete();
        }

        public void OnSetLeaderboardScoreError(string error)
        {
            YandexGames.OnSetLeaderboardScoreError(error);
        }

        public void OnGetLeaderboardDescriptionComplete(string json)
        {
            YandexGames.OnGetLeaderboardDescriptionComplete(json);
        }

        public void OnGetLeaderboardDescriptionError(string error)
        {
            YandexGames.OnGetLeaderboardDescriptionError(error);
        }

        public void OnGetLeaderboardPlayerEntryComplete(string json)
        {
            YandexGames.OnGetLeaderboardPlayerEntryComplete(json);
        }

        public void OnGetLeaderboardPlayerEntryError(string error)
        {
            YandexGames.OnGetLeaderboardPlayerEntryError(error);
        }

        public void OnGetLeaderboardEntriesComplete(string json)
        {
            YandexGames.OnGetLeaderboardEntriesComplete(json);
        }

        public void OnGetLeaderboardEntriesError(string error)
        {
            YandexGames.OnGetLeaderboardEntriesError(error);
        }

        // Remote Config callbacks
        public void OnGetFlagsComplete(string json)
        {
            YandexGames.OnGetFlagsComplete(json);
        }

        public void OnGetFlagsError(string error)
        {
            YandexGames.OnGetFlagsError(error);
        }

        // Review callbacks
        public void OnCanReviewComplete(string json)
        {
            YandexGames.OnCanReviewComplete(json);
        }

        public void OnCanReviewError(string error)
        {
            YandexGames.OnCanReviewError(error);
        }

        public void OnRequestReviewComplete(string json)
        {
            YandexGames.OnRequestReviewComplete(json);
        }

        public void OnRequestReviewError(string error)
        {
            YandexGames.OnRequestReviewError(error);
        }
    }
}
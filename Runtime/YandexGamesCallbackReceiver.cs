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

        public void OnSaveDataComplete(string key)
        {
            YandexGames.OnSaveDataComplete(key);
        }

        public void OnSaveDataError(string json)
        {
            YandexGames.OnSaveDataError(json);
        }

        public void OnLoadDataComplete(string json)
        {
            YandexGames.OnLoadDataComplete(json);
        }

        public void OnLoadDataError(string json)
        {
            YandexGames.OnLoadDataError(json);
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
        public void OnSetLeaderboardScoreComplete(string leaderboardName)
        {
            YandexGames.OnSetLeaderboardScoreComplete(leaderboardName);
        }

        public void OnSetLeaderboardScoreError(string json)
        {
            YandexGames.OnSetLeaderboardScoreError(json);
        }

        public void OnGetLeaderboardDescriptionComplete(string json)
        {
            YandexGames.OnGetLeaderboardDescriptionComplete(json);
        }

        public void OnGetLeaderboardDescriptionError(string json)
        {
            YandexGames.OnGetLeaderboardDescriptionError(json);
        }

        public void OnGetLeaderboardPlayerEntryComplete(string json)
        {
            YandexGames.OnGetLeaderboardPlayerEntryComplete(json);
        }

        public void OnGetLeaderboardPlayerEntryError(string json)
        {
            YandexGames.OnGetLeaderboardPlayerEntryError(json);
        }

        public void OnGetLeaderboardEntriesComplete(string json)
        {
            YandexGames.OnGetLeaderboardEntriesComplete(json);
        }

        public void OnGetLeaderboardEntriesError(string json)
        {
            YandexGames.OnGetLeaderboardEntriesError(json);
        }

        // Remote Config callbacks
        public void OnGetFlagsComplete(string json)
        {
            YandexGames.OnGetFlagsComplete(json);
        }

        public void OnGetFlagsError(string json)
        {
            YandexGames.OnGetFlagsError(json);
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

        // Payment callbacks
        public void OnGetCatalogComplete(string json)
        {
            YandexGamesPayments.OnGetCatalogComplete(json);
        }

        public void OnGetCatalogError(string error)
        {
            YandexGamesPayments.OnGetCatalogError(error);
        }

        public void OnPurchaseComplete(string json)
        {
            YandexGamesPayments.OnPurchaseComplete(json);
        }

        public void OnPurchaseError(string error)
        {
            YandexGamesPayments.OnPurchaseError(error);
        }

        public void OnGetPurchasesComplete(string json)
        {
            YandexGamesPayments.OnGetPurchasesComplete(json);
        }

        public void OnGetPurchasesError(string error)
        {
            YandexGamesPayments.OnGetPurchasesError(error);
        }

        public void OnConsumePurchaseComplete(string empty)
        {
            YandexGamesPayments.OnConsumePurchaseComplete(empty);
        }

        public void OnConsumePurchaseError(string error)
        {
            YandexGamesPayments.OnConsumePurchaseError(error);
        }
    }
}
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
    }
}
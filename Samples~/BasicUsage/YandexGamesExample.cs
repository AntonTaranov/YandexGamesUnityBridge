using UnityEngine;
using YandexGames;
using Cysharp.Threading.Tasks;

/// <summary>
/// Example script demonstrating minimal usage of YandexGames plugin
/// </summary>
public class YandexGamesExample : MonoBehaviour
{
    [SerializeField] private bool autoGetPlayerData = true;
    [SerializeField] private bool testCloudStorage = true;
    
    private async void Start()
    {
        // Plugin is automatically initialized on game start
        await UniTask.WaitUntil(() => YandexGames.YandexGames.IsInitialized);
        
        Debug.Log("YandexGames plugin is ready!");
        
        if (autoGetPlayerData)
        {
            await GetPlayerData();
        }
        
        if (testCloudStorage)
        {
            await TestCloudStorage();
        }
    }
    
    private async UniTask GetPlayerData()
    {
        try
        {
            var playerData = await YandexGames.YandexGames.GetPlayerDataAsync();
            Debug.Log($"Player: {playerData.name}, Language: {playerData.lang}, Device: {playerData.device}");
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to get player data: {ex.Message}");
        }
    }
    
    private async UniTask TestCloudStorage()
    {
        try
        {
            // Save some test data
            var testData = JsonUtility.ToJson(new { score = 100, level = 5 });
            await YandexGames.YandexGames.SaveDataAsync("gameProgress", testData);
            Debug.Log("Data saved successfully");
            
            // Load it back
            var loadedData = await YandexGames.YandexGames.LoadDataAsync("gameProgress");
            Debug.Log($"Loaded data: {loadedData}");
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Cloud storage error: {ex.Message}");
        }
    }
    
    // Call this method to show an interstitial ad
    public async void ShowInterstitialAd()
    {
        try
        {
            await YandexGames.YandexGames.ShowInterstitialAdAsync();
            Debug.Log("Interstitial ad completed");
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Interstitial ad error: {ex.Message}");
        }
    }
    
    // Call this method to show a rewarded ad
    public async void ShowRewardedAd()
    {
        try
        {
            await YandexGames.YandexGames.ShowRewardedAdAsync(rewarded =>
            {
                if (rewarded)
                {
                    Debug.Log("Player was rewarded!");
                    // Give reward to player here
                }
                else
                {
                    Debug.Log("Player did not complete the ad");
                }
            });
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Rewarded ad error: {ex.Message}");
        }
    }
}
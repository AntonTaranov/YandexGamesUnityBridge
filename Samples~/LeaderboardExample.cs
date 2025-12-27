using UnityEngine;
using YandexGames;
using YandexGames.Leaderboards;
using Cysharp.Threading.Tasks;

/// <summary>
/// Example demonstrating Yandex Games Leaderboard integration
/// </summary>
public class LeaderboardExample : MonoBehaviour
{
    [SerializeField] private string leaderboardName = "YourLeaderboardName";
    
    private async void Start()
    {
        // Wait for Yandex Games SDK initialization
        while (!YandexGames.YandexGames.IsInitialized)
        {
            await UniTask.Delay(100);
        }
        
        Debug.Log("Yandex Games initialized - ready to use leaderboards");
    }
    
    /// <summary>
    /// Submit a score to the leaderboard
    /// </summary>
    public async void SubmitScore(int score)
    {
        try
        {
            await YandexGames.YandexGames.SetLeaderboardScoreAsync(leaderboardName, score);
            Debug.Log($"Score {score} submitted successfully!");
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to submit score: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get current player's rank and score
    /// </summary>
    public async void GetMyRank()
    {
        try
        {
            var entry = await YandexGames.YandexGames.GetLeaderboardPlayerEntryAsync(leaderboardName);
            Debug.Log($"Your rank: {entry.rank + 1}, Score: {entry.formattedScore}");
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to get player entry: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Display top 10 players
    /// </summary>
    public async void ShowTop10()
    {
        try
        {
            var response = await YandexGames.YandexGames.GetLeaderboardEntriesAsync(
                leaderboardName,
                includeUser: true,
                quantityTop: 10
            );
            
            Debug.Log($"=== {response.leaderboard.title.en} ===");
            foreach (var entry in response.entries)
            {
                Debug.Log($"#{entry.rank + 1}: {entry.player.publicName} - {entry.formattedScore}");
            }
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to get leaderboard entries: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get leaderboard metadata
    /// </summary>
    public async void GetLeaderboardInfo()
    {
        try
        {
            var description = await YandexGames.YandexGames.GetLeaderboardDescriptionAsync(leaderboardName);
            Debug.Log($"Leaderboard: {description.title.en}");
            Debug.Log($"Score Type: {description.description.score_format.type}");
            Debug.Log($"Sort: {(description.description.invert_sort_order ? "Ascending" : "Descending")}");
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to get leaderboard description: {ex.Message}");
        }
    }
}

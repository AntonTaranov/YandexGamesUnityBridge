using UnityEngine;
using YandexGames;
using YandexGames.RemoteConfig;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Example demonstrating Yandex Games Remote Configuration integration
/// </summary>
public class RemoteConfigExample : MonoBehaviour
{
    private async void Start()
    {
        // Wait for Yandex Games SDK initialization
        while (!YandexGames.YandexGames.IsInitialized)
        {
            await UniTask.Delay(100);
        }
        
        Debug.Log("Yandex Games initialized - ready to use remote config");
        
        // Example: Load feature flags on startup
        await LoadFeatureFlags();
    }
    
    /// <summary>
    /// Load feature flags with default values
    /// </summary>
    public async UniTask LoadFeatureFlags()
    {
        try
        {
            // Define default flags
            var defaultFlags = new Dictionary<string, string>
            {
                { "showNewUI", "false" },
                { "enablePowerUps", "true" },
                { "difficultyLevel", "normal" }
            };
            
            // Optional: Send player context for targeting
            var clientFeatures = new ClientFeature[]
            {
                new ClientFeature("playerLevel", "15"),
                new ClientFeature("payingUser", "yes")
            };
            
            var flags = await YandexGames.YandexGames.GetFlagsAsync(defaultFlags, clientFeatures);
            
            // Use flags to configure game features
            Debug.Log($"Show New UI: {flags["showNewUI"]}");
            Debug.Log($"Enable Power-Ups: {flags["enablePowerUps"]}");
            Debug.Log($"Difficulty: {flags["difficultyLevel"]}");
            
            // Apply flags to game logic
            ApplyFeatureFlags(flags);
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to load feature flags: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Apply feature flags to game configuration
    /// </summary>
    private void ApplyFeatureFlags(Dictionary<string, string> flags)
    {
        if (flags.TryGetValue("showNewUI", out var showNewUI) && showNewUI == "true")
        {
            // Enable new UI features
            Debug.Log("New UI enabled via remote config");
        }
        
        if (flags.TryGetValue("enablePowerUps", out var enablePowerUps) && enablePowerUps == "true")
        {
            // Enable power-up system
            Debug.Log("Power-ups enabled via remote config");
        }
        
        if (flags.TryGetValue("difficultyLevel", out var difficulty))
        {
            // Adjust game difficulty
            Debug.Log($"Difficulty set to: {difficulty}");
        }
    }
    
    /// <summary>
    /// Example: A/B test for new feature
    /// </summary>
    public async void CheckABTest()
    {
        try
        {
            var defaultFlags = new Dictionary<string, string>
            {
                { "experimentVariant", "control" }
            };
            
            var flags = await YandexGames.YandexGames.GetFlagsAsync(defaultFlags);
            
            var variant = flags["experimentVariant"];
            Debug.Log($"A/B Test Variant: {variant}");
            
            if (variant == "variantA")
            {
                // Show variant A experience
                Debug.Log("Showing variant A");
            }
            else if (variant == "variantB")
            {
                // Show variant B experience
                Debug.Log("Showing variant B");
            }
            else
            {
                // Show control experience
                Debug.Log("Showing control");
            }
        }
        catch (YandexGamesException ex)
        {
            Debug.LogError($"Failed to check A/B test: {ex.Message}");
        }
    }
}

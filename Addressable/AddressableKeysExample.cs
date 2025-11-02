/// <summary>
/// 🔑 Addressable Keys - Type-safe string constants
/// Prevents typos and makes refactoring easier
/// </summary>
public static class AddressableKeys
{
    #region 🎮 Gameplay
    
    public static class Gameplay
    {
        // Players
        public const string PlayerDefault = "Player_Default";
        public const string PlayerPremium = "Player_Premium";
        
        // Enemies
        public const string EnemyBasic = "Enemy_Basic";
        public const string EnemyFast = "Enemy_Fast";
        public const string EnemyTank = "Enemy_Tank";
        public const string EnemyBoss = "Enemy_Boss";
        
        // Collectibles
        public const string CoinGold = "Collectible_Coin_Gold";
        public const string CoinSilver = "Collectible_Coin_Silver";
        public const string GemBlue = "Collectible_Gem_Blue";
        public const string GemRed = "Collectible_Gem_Red";
        
        // Power-ups
        public const string PowerUpSpeed = "PowerUp_Speed";
        public const string PowerUpShield = "PowerUp_Shield";
        public const string PowerUpMagnet = "PowerUp_Magnet";
    }
    
    #endregion
    
    #region 🎨 UI
    
    public static class UI
    {
        // Menus
        public const string MainMenu = "UI_MainMenu";
        public const string GameplayHUD = "UI_GameplayHUD";
        public const string PauseMenu = "UI_PauseMenu";
        public const string SettingsMenu = "UI_SettingsMenu";
        
        // Popups
        public const string PopupWin = "UI_Popup_Win";
        public const string PopupLose = "UI_Popup_Lose";
        public const string PopupReward = "UI_Popup_Reward";
        public const string PopupShop = "UI_Popup_Shop";
        
        // Icons
        public const string IconCoin = "UI_Icon_Coin";
        public const string IconGem = "UI_Icon_Gem";
        public const string IconStar = "UI_Icon_Star";
        public const string IconHeart = "UI_Icon_Heart";
        
        // Buttons
        public const string ButtonGreen = "UI_Button_Green";
        public const string ButtonRed = "UI_Button_Red";
        public const string ButtonBlue = "UI_Button_Blue";
    }
    
    #endregion
    
    #region 🎵 Audio
    
    public static class Audio
    {
        // BGM(BackGroundMusic)
        public const string BGM_Menu = "BGM_MainMenu";
        public const string BGM_Gameplay = "BGM_Gameplay";
        public const string BGM_Boss = "BGM_BossFight";
        
        // SFX
        public const string SFX_Click = "SFX_UI_Click";
        public const string SFX_Coin = "SFX_Coin_Collect";
        public const string SFX_Win = "SFX_Victory";
        public const string SFX_Lose = "SFX_GameOver";
        public const string SFX_Jump = "SFX_Player_Jump";
        public const string SFX_Shoot = "SFX_Player_Shoot";
        public const string SFX_Hit = "SFX_Enemy_Hit";
        public const string SFX_Explosion = "SFX_Explosion";
    }
    
    #endregion
    
    #region ✨ VFX
    
    public static class VFX
    {
        public const string ExplosionSmall = "VFX_Explosion_Small";
        public const string ExplosionBig = "VFX_Explosion_Big";
        public const string SparklesGold = "VFX_Sparkles_Gold";
        public const string SparklesBlue = "VFX_Sparkles_Blue";
        public const string TrailRed = "VFX_Trail_Red";
        public const string TrailBlue = "VFX_Trail_Blue";
        public const string HitEffect = "VFX_Hit_Impact";
        public const string LevelUp = "VFX_LevelUp";
    }
    
    #endregion
    
    #region 🗺️ Scenes
    
    public static class Scenes
    {
        public const string MainMenu = "Scene_MainMenu";
        public const string Loading = "Scene_Loading";
        
        // Levels
        public const string Level_01 = "Scene_Level_01";
        public const string Level_02 = "Scene_Level_02";
        public const string Level_03 = "Scene_Level_03";
        
        // Format helper
        public static string GetLevel(int levelNum) => $"Scene_Level_{levelNum:D2}";
    }
    
    #endregion
    
    #region 🎁 IAP / Shop
    
    public static class Shop
    {
        // Coin packs
        public const string CoinPack_Small = "IAP_Coins_100";
        public const string CoinPack_Medium = "IAP_Coins_500";
        public const string CoinPack_Large = "IAP_Coins_1000";
        
        // Gem packs
        public const string GemPack_Small = "IAP_Gems_50";
        public const string GemPack_Medium = "IAP_Gems_200";
        public const string GemPack_Large = "IAP_Gems_500";
        
        // Special
        public const string RemoveAds = "IAP_RemoveAds";
        public const string StarterPack = "IAP_StarterPack";
    }
    
    #endregion
    
    #region 🏷️ Labels (Batch Loading)
    
    public static class Labels
    {
        // Essential (load at startup)
        public const string Essential = "Essential";
        public const string UI_Core = "UI_Core";
        
        // Level-specific
        public const string Level_Tutorial = "Level_Tutorial";
        public const string Level_Forest = "Level_Forest";
        public const string Level_Desert = "Level_Desert";
        public const string Level_Snow = "Level_Snow";
        
        // Themes
        public const string Theme_Default = "Theme_Default";
        public const string Theme_Halloween = "Theme_Halloween";
        public const string Theme_Christmas = "Theme_Christmas";
        
        // Events
        public const string Event_Summer = "Event_Summer";
        public const string Event_Winter = "Event_Winter";
        
        // Format helpers
        public static string GetLevelTheme(string themeName) => $"Level_{themeName}";
        public static string GetEvent(string eventName) => $"Event_{eventName}";
    }
    
    #endregion
    
    #region 🎭 Skins
    
    public static class Skins
    {
        // Player skins
        public const string Player_Default = "Skin_Player_Default";
        public const string Player_Knight = "Skin_Player_Knight";
        public const string Player_Ninja = "Skin_Player_Ninja";
        public const string Player_Pirate = "Skin_Player_Pirate";
        
        // Weapon skins
        public const string Weapon_Default = "Skin_Weapon_Default";
        public const string Weapon_Fire = "Skin_Weapon_Fire";
        public const string Weapon_Ice = "Skin_Weapon_Ice";
        
        // Format helper
        public static string GetPlayerSkin(int skinId) => $"Skin_Player_{skinId:D2}";
    }
    
    #endregion
    
    #region 🏆 Achievements
    
    public static class Achievements
    {
        public const string Icon_Bronze = "Achievement_Bronze";
        public const string Icon_Silver = "Achievement_Silver";
        public const string Icon_Gold = "Achievement_Gold";
        public const string Icon_Platinum = "Achievement_Platinum";
    }
    
    #endregion
}

/// <summary>
/// 🛠️ Usage Examples
/// </summary>
public class KeyUsageExamples
{
    private async void LoadExamples()
    {
        var mgr = AddressableManager.Instance;
        
        // ✓ Type-safe, no typos
        var coin = await mgr.LoadAsync<UnityEngine.GameObject>(AddressableKeys.Gameplay.CoinGold);
        
        // ✓ Easy to find with IntelliSense
        var clickSound = await mgr.LoadAsync<UnityEngine.AudioClip>(AddressableKeys.Audio.SFX_Click);
        
        // ✓ Batch loading by label
        var forestAssets = await mgr.LoadByLabelAsync<UnityEngine.GameObject>(
            AddressableKeys.Labels.Level_Forest, 
            null
        );
        
        // ✓ Dynamic key generation
        var levelScene = AddressableKeys.Scenes.GetLevel(5); // "Scene_Level_05"
        var playerSkin = AddressableKeys.Skins.GetPlayerSkin(3); // "Skin_Player_03"
    }
}
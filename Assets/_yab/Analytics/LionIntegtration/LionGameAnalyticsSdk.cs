#if HAS_LION_GAME_ANALYTICS_SDK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using LionStudios;
using LionStudios.Suite.Debugging;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Setup;
using GameAnalyticsSDK.State;
using GameAnalyticsSDK.Wrapper;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

#if UNITY_IOS
using GameAnalyticsSDK.iOS;
#endif

public class LionGameAnalyticsSdk : ILionSdk, IGameAnalyticsATTListener
{
    private static GameAnalyticsSettings _settings = new GameAnalyticsSettings();

    string cachedGameKey;

    public class GameAnalyticsSettings : ILionSettingsInfo
    {
        [Header("Make sure to add in the GameAnalytics Prefab (Window/GameAnalytics/Create GameAnalytics Object)")]
        public string iOSGameKey = "";
        public string iOSSecretKey = "";

        [Header("Android Keys")]
        public string androidGameKey = "";
        public string androidSecretKey = "";
    }

    private string gameKey
    {
        get
        {
#if UNITY_IOS
            return _settings.iOSGameKey;
#elif UNITY_ANDROID
            return _settings.androidGameKey;
#endif
        }
    }

    private string secretKey
    {
        get
        {
#if UNITY_IOS
            return _settings.iOSSecretKey;
#elif UNITY_ANDROID
            return _settings.androidSecretKey;
#endif
        }
    }

    public int Priority => 1;

    public void ApplySettings(ILionSettingsInfo newSettings)
    {
        _settings = (GameAnalyticsSettings)newSettings;
    }

    public ILionSettingsInfo GetSettings()
    {
        if (_settings == null)
        {
            _settings = new GameAnalyticsSettings();
        }

#if UNITY_EDITOR
    if(cachedGameKey == null || (cachedGameKey != gameKey && gameKey != null)){
       changeDb();
       cachedGameKey = gameKey;
    }
#endif
        return _settings;
    }

#if UNITY_EDITOR
    public void changeDb()
    {
        var settingsObject = AssetDatabase.LoadAssetAtPath<GameAnalyticsSDK.Setup.Settings>("Assets/Resources/GameAnalytics/Settings.asset");
        if (settingsObject != null)
        {
            UnityEditor.SerializedObject serializedGaSettings = new SerializedObject(settingsObject);

            serializedGaSettings.FindProperty("gameKey").GetArrayElementAtIndex(0).stringValue = gameKey;
            serializedGaSettings.FindProperty("secretKey").GetArrayElementAtIndex(0).stringValue = secretKey;
            
            serializedGaSettings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
#endif

    public string[] GetPrivacyLinks()
    {
        return new string[] { "https://gameanalytics.com/privacy/" };
    }

    private bool _isInitialized;
    public bool IsInitialized()
    {
        return _isInitialized;
    }

    //ATT Initialization
    public void GameAnalyticsATTListenerNotDetermined()
    {
        return;
    }
    
    public void GameAnalyticsATTListenerRestricted()
    {
        return;
    }
    
    public void GameAnalyticsATTListenerDenied()
    {
        return;
    }
    
    public void GameAnalyticsATTListenerAuthorized()
    {
        return;
    }

    public void OnInitialize(LionCoreContext ctx)
    {
        GameAnalytics.Initialize();
    }

    public void OnPostInitialize(LionCoreContext ctx) { }

    public void OnPreInitialize(LionCoreContext ctx)
    {
        // Checks if iPhone
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
    }
}
#endif
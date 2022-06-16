#if HAS_LION_ADJUST_SDK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using LionStudios.Suite.Analytics;
using LionStudios;
using LionStudios.Suite.Debugging;
using com.adjust.sdk;


/// <summary>
/// This is a sample script for integrating Adjust with Lion Suite.
/// Includes settings and functionality for initializing Adjust SDK.
/// 
/// NOTE - REQUIRES PACKAGES:
///     Adjust SDK
///     Lion Suite - Analytics
/// </summary>
public class LionAdjustSdk : ILionSdk
{
    private static AdjustSettings _settings = new AdjustSettings();
    public class AdjustSettings : ILionSettingsInfo
    {
        public string iOSToken = "";
        public string androidToken = "";
        public AdjustEnvironment environment;
    }


    private string appToken
    {
        get
        {
#if UNITY_IOS
            return _settings.iOSToken;
#elif UNITY_ANDROID
            return _settings.androidToken;
#endif
        }
    }

    public int Priority => 1;

    public void ApplySettings(ILionSettingsInfo newSettings)
    {
        _settings = (AdjustSettings)newSettings;
    }

    public ILionSettingsInfo GetSettings()
    {
        if(_settings == null)
        {
            _settings = new AdjustSettings();
        }

        return _settings;
    }

    public string[] GetPrivacyLinks()
    {
        return new string[] { "https://www.applovin.com/privacy/" };
    }

    private bool _isInitialized;
    public bool IsInitialized()
    {
        return _isInitialized;
    }

    public void OnInitialize(LionCoreContext ctx)
    {
        AdjustConfig config = new AdjustConfig(appToken, _settings.environment);
        
        var lionLogLevel = ctx.GetSettings<LionDebug.LionDebugSettings>().debugLogLevel;
        AdjustLogLevel adjustLogLevel = AdjustLogLevel.Info;
        switch (lionLogLevel)
        {
            case LionDebug.DebugLogLevel.Default:
            case LionDebug.DebugLogLevel.Event:
                adjustLogLevel = AdjustLogLevel.Info;
                break;

            case LionDebug.DebugLogLevel.Warn:
                adjustLogLevel = AdjustLogLevel.Warn;
                break;

            case LionDebug.DebugLogLevel.None:
                adjustLogLevel = AdjustLogLevel.Suppress;
                break;

            case LionDebug.DebugLogLevel.Error:
                adjustLogLevel = AdjustLogLevel.Error;
                break;

            case LionDebug.DebugLogLevel.Verbose:
                adjustLogLevel = AdjustLogLevel.Verbose;
                break;
        }

        config.setLogLevel(adjustLogLevel);
        config.setLogDelegate(msg => LionDebug.Log(msg));
        new GameObject("Adjust").AddComponent<Adjust>();
        Adjust.start(config);
        _isInitialized = true;
    }

    public void OnPostInitialize(LionCoreContext ctx)
    {
    }

    public void OnPreInitialize(LionCoreContext ctx)
    {
    }
}
#endif
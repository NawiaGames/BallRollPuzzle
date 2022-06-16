#if HAS_LION_APPLOVIN_SDK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using LionStudios.Suite.Debugging;
using System;

/// <summary>
/// This is a sample script for integrating MAX SDK with Lion Suite.
/// For loading and displaying ads, refer to the sample script with ILionAdProvider and GDPR
/// Includes settings and functionality for initializing MAX.
/// 
/// NOTE - REQUIRES PACKAGES:
///     AppLovin MAX
/// </summary>
public class LionMaxSdk : ILionSdk
{
    public int Priority => 0;

    private static ApplovinMaxSettings _settings = new ApplovinMaxSettings();
    
    [LabelOverride("AppLovin MAX")]
    public class ApplovinMaxSettings : ILionSettingsInfo
    {
        [Header("General")]
        public string sdkKey = "";
    }

    public void ApplySettings(ILionSettingsInfo newSettings)
    {
        _settings = (ApplovinMaxSettings)newSettings;
    }

    public ILionSettingsInfo GetSettings()
    {
        if (_settings == null)
        {
            _settings = new ApplovinMaxSettings();
        }

        return _settings;
    }

    public bool IsInitialized()
    {
        return MaxSdk.IsInitialized();
    }

    private void OnMaxInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
    {
        if (MaxSdk.IsInitialized())
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            MaxSdk.ShowMediationDebugger();
#endif
            Debug.Log("MaxSDK init Complete.  Consent Dialog State = " + sdkConfiguration.ConsentDialogState);
        }
        else
        {
            Debug.Log("Failed to init MaxSDK");
        }
    }

    public void OnInitialize(LionCoreContext ctx)
    {
        ApplovinMaxSettings maxSettings = ctx.GetSettings<ApplovinMaxSettings>();
        LionDebug.LionDebugSettings debugSettings = ctx.GetSettings<LionDebug.LionDebugSettings>();

        // init callback
        MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;
        
        MaxSdk.SetSdkKey(maxSettings.sdkKey);
        MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
        MaxSdk.SetVerboseLogging(debugSettings.debugLogLevel == LionDebug.DebugLogLevel.Verbose);
        MaxSdk.InitializeSdk();
    }

    public void OnPostInitialize(LionCoreContext ctx) { }
    public void OnPreInitialize(LionCoreContext ctx) { }

#region Privacy Links
    private Dictionary<string, string> _privacyLinks = new Dictionary<string, string>
    {
        {"UNITY_NETWORK", "https://unity3d.com/fr/legal/privacy-policy" },
        {"APPLOVIN", "https://www.applovin.com/privacy/" },
        {"ADMOB_NETWORK", "https://policies.google.com/privacy/update" },
        {"ADCOLONY_NETWORK", "https://www.adcolony.com/privacy-policy/" },
        {"AMAZON_NETWORK", "https://advertising.amazon.com/resources/ad-policy/en/gdpr" },
        {"CHARTBOOST_NETWORK", "https://answers.chartboost.com/en-us/articles/200780269" },
        {"FYBER_NETWORK", "https://fyber.com/Privacy-policy/" },
        {"INMOBI_NETWORK", "https://www.inmobi.com/privacy-policy/" },
        {"IRONSOURCE_NETWORK", "https://ironsource.mobi/privacypolicy.html" },
        {"MINTEGRAL_NETWORK", "https://www.mintegral.com/en/privacy/" },
        {"NEND_NETWORK", "https://www.fancs.com/en/privacy" },
        {"SMAATO_NETWORK", "https://www.smaato.com/privacy/" },
        {"TIKTOK_NETWORK", "https://www.tiktok.com/legal/privacy-policy?lang=en#privacy-eea" },
        {"VERIZON_NETWORK", "https://www.verizon.com/about/privacy/advertising-programs-privacy-notice" },
        {"YANDEX_NETWORK", "https://yandex.com/legal/confidential/" },
        {"VUNGLE_NETWORK", "https://vungle.com/privacy/" },
        {"FACEBOOK_MEDIATE", "https://www.facebook.com/policy.php" },
        {"MYTARGET_NETWORK", "https://legal.my.com/us/mytarget/privacy/" }
    };

    public string[] GetPrivacyLinks()
    {
        List<string> privacyLinks = new List<string>();
        foreach (var kvp in _privacyLinks)
        {
            privacyLinks.Add(kvp.Value);
        }

        return privacyLinks.ToArray();
    }
#endregion
}
#endif
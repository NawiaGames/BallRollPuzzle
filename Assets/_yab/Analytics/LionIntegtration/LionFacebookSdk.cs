#if HAS_LION_FACEBOOK_SDK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using LionStudios.Suite.Analytics;
using LionStudios;
using LionStudios.Suite.Debugging;
using Facebook.Unity;
using System.Runtime.InteropServices;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

#if UNITY_IOS
namespace AudienceNetwork
{
    public static class AdSettings
    {
        [DllImport("__Internal")] 
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
        }
    }
}
#endif

public class LionFbSdk : ILionSdk
{
  private static FacebookSdkSettings _settings = new FacebookSdkSettings();

  string cachedapp_ID;

  [LabelOverride("Facebook SDK")]
  public class FacebookSdkSettings : ILionSettingsInfo
  {
    public string app_ID = "";
  }

  public int Priority => 1;

  public void ApplySettings(ILionSettingsInfo newSettings)
  {
    _settings = (FacebookSdkSettings)newSettings;
  }

  public ILionSettingsInfo GetSettings()
  {
    if(_settings == null)
    {
      _settings = new FacebookSdkSettings();
    }


#if UNITY_EDITOR
    if(cachedapp_ID != _settings.app_ID && _settings.app_ID != null)
    {
      changeDb();
      cachedapp_ID = _settings.app_ID;
    }
#endif

    return _settings;
  }

#if UNITY_EDITOR
  public static void changeDb()
  {
    var settingsObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/FacebookSDK/SDK/Resources/FacebookSettings.asset");
    UnityEditor.SerializedObject serializedFbSettings = new SerializedObject(settingsObject);
    serializedFbSettings.FindProperty("appIds").GetArrayElementAtIndex(0).stringValue = _settings.app_ID;
    //Debug.Log(serializedFbSettings.FindProperty("appIds").GetArrayElementAtIndex(0).stringValue);
    serializedFbSettings.ApplyModifiedProperties();
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
  }
#endif


  public string[] GetPrivacyLinks()
  {
    return new string[] { "https://www.facebook.com/policy.php" };
  }


  private bool _isInitialized;
  public bool IsInitialized()
  {
    return _isInitialized;
  }

  private void InitCallback()
  {
    if(FB.IsInitialized)
    {
      FB.ActivateApp();
    }
    else
    {
      LionDebug.Log("Failed to Initialize the Facebook SDK");
    }

  }


  private void OnHideUnity(bool isGameShown)
  {
    if(!isGameShown)
    {
      // Pause the game - we will need to hide
      Time.timeScale = 0;
    }
    else
    {
      // Resume the game - we're getting focus again
      Time.timeScale = 1;
    }
  }

  public void FacebookLionSdkInitialize()
  {
    if(!FB.IsInitialized)
    {
      // Initialize the Facebook SDK
      FB.Init(InitCallback, OnHideUnity);
    }
    else
    {
      // Already initialized, signal an app activation App Event
      FB.ActivateApp();
    }
  }

  public void OnInitialize(LionCoreContext ctx)
  {
    FacebookLionSdkInitialize();
  }

  public void OnPostInitialize(LionCoreContext ctx) { }

  public void OnPreInitialize(LionCoreContext ctx)
  {
#if UNITY_IOS && HAS_LION_APPLOVIN_SDK
        MaxSdkCallbacks.OnSdkInitializedEvent += (sdkConfiguration) => {
            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized);
            FB.Mobile.SetAdvertiserTrackingEnabled(sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized);
        };
#endif
  }

}
#endif
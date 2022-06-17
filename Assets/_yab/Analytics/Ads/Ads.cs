using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if HAS_LION_APPLOVIN_SDK
//using LionStudios.Suite.Ads;
#endif
public class Ads : MonoBehaviour
{
  public static System.Action<string> onInvokeInters, onShowInters, onIntersHidden;
  public static System.Action<string> onInvokeRewarded, onShowRewarded, onRewarded, onRewardHidden;

#if HAS_LION_APPLOVIN_SDK
  LionMaxSdk maxsdk = null;
#endif  

  static string reward_placement = "";
  static string inters_placement = "";

  static Ads static_ads;
  public static Ads get()
  {
    return static_ads;
  }
  Ads()
  {
    static_ads = this;
  }
  void Awake()
  {
    static_ads = this;

#if HAS_LION_APPLOVIN_SDK
    maxsdk = new LionMaxSdk();

    MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdk.SdkConfiguration conf)=>
    {
      // LionAds.LoadInterstitial();
      // LionAds.LoadRewarded();
    };
    MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (string adID, MaxSdk.AdInfo adInfo) =>
    {
  #if UNITY_EDITOR
      onShowRewarded?.Invoke(reward_placement);
  #else
      onShowRewarded?.Invoke(adInfo.Placement);
  #endif
    };
    MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (string adID, MaxSdk.ErrorInfo errorInfo, MaxSdk.AdInfo adInfo) =>
    {
  #if UNITY_EDITOR
      onRewardHidden?.Invoke(reward_placement);
  #else
      onRewardHidden?.Invoke(adInfo.Placement);
  #endif
    };
    MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (string adID, MaxSdk.AdInfo adInfo) =>
    {
      //Debug.Log("AD| " + adID + "," + adInfo.Placement + "," + adInfo.Revenue);
    };
    MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (string adID, MaxSdk.Reward reward, MaxSdk.AdInfo adInfo) =>
    {
  #if UNITY_EDITOR
      onRewarded?.Invoke(reward_placement);
  #else
      onRewarded?.Invoke(adInfo.Placement);
  #endif
    };
    MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (string adID, MaxSdk.AdInfo adInfo) =>
    {
  #if UNITY_EDITOR
      onRewardHidden?.Invoke(reward_placement);
  #else
      onRewardHidden?.Invoke(adInfo.Placement);
  #endif
    };
    MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (string adID, MaxSdk.AdInfo adInfo) =>
    {
  #if UNITY_EDITOR
      onShowInters?.Invoke(inters_placement);
  #else
      onShowInters?.Invoke(adInfo.Placement);
  #endif
    };
    this.Invoke(()=>
    {
      // LionAds.LoadInterstitial();
      // LionAds.LoadRewarded();
    }, 5.0f);
#endif
  }
  void OnDestroy()
  {
    reward_placement = null;
		static_ads = null;
  }

  public static bool IsIntersReady()
  {
  #if HAS_LION_APPLOVIN_SDK
    return false; //get()?.maxsdk?.IsAdReady(LionAdType.Interstitial) ?? false;
  #else
    return false;
  #endif
  }
  public static void ShowInters(string placement)
  {
  #if HAS_LION_APPLOVIN_SDK
    if(IsIntersReady())
    {
      inters_placement = placement;
      onInvokeInters?.Invoke(placement);
      //LionAds.ShowInterstitial<LionMaxSdk>(placement, 1);
    }
    else
    {
      //LionAds.LoadInterstitial();
    }
  #endif    
  }
  public static bool IsRewardReady()
  {
#if HAS_LION_APPLOVIN_SDK
    // bool is_ready = get()?.maxsdk?.IsAdReady(LionAdType.Rewarded) ?? false;
    // if(!is_ready)
    //   LionAds.LoadRewarded();
    // return is_ready;
    return false;
#else
    return false;
#endif
  }
  public static void ShowRewarded(string placement)
  {
#if HAS_LION_APPLOVIN_SDK
    if(IsRewardReady())
    {
      reward_placement = placement;
      //LionAds.ShowRewarded<LionMaxSdk>(placement);
      onInvokeRewarded?.Invoke(placement);      
    }
    else
    {
      //LionAds.LoadRewarded();
    }
#endif
  }

#if UNITY_EDITOR
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.A))
        ShowInters("Custom");
    }
#endif
}

#if HAS_LION_GAME_ANALYTICS_SDK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Analytics;
using GameAnalyticsSDK;
#if HAS_BYTEBREW_SDK
using ByteBrewSDK;
#endif

public class AnalyticsEvents : MonoBehaviour
{
  void Awake()
  {
  #if HAS_BYTEBREW_SDK
    ByteBrewInit();
  #endif  

    Level.onStart += on_level_started;
    Level.onFinished += on_level_finished;
    Game.onLevelRestart += on_level_restarted;

    // Ads.onInvokeRewarded += on_invoke_rewarded_ad;
    // Ads.onShowRewarded += on_show_rewarded_ad;
    // Ads.onShowInters += on_show_inters_ad;
  }
  void OnDestroy()
  {
    Level.onStart -= on_level_started;
    Level.onFinished -= on_level_finished;
    Game.onLevelRestart -= on_level_restarted;

    // Ads.onInvokeRewarded -= on_invoke_rewarded_ad;
    // Ads.onShowRewarded -= on_show_rewarded_ad;
    // Ads.onShowInters -= on_show_inters_ad;
  }

#if HAS_BYTEBREW_SDK
  void ByteBrewInit()
  {
  #if UNITY_IOS
    ByteBrew.requestForAppTrackingTransparency((status) =>
    {
      //Case 0: ATTrackingManagerAuthorizationStatusAuthorized
      //Case 1: ATTrackingManagerAuthorizationStatusDenied
      //Case 2: ATTrackingManagerAuthorizationStatusRestricted
      //Case 3: ATTrackingManagerAuthorizationStatusNotDetermined
      Debug.Log("ByteBrew Got a status of: " + status);
      ByteBrew.InitializeByteBrew();
    });
  #else
    ByteBrew.InitializeByteBrew();
  #endif
  }
#endif

  void on_level_started(Level level)
  {
    LionAnalytics.LevelStart(GameState.Progress.Level + 1, 0);
  #if HAS_BYTEBREW_SDK
    ByteBrew.NewProgressionEvent(ByteBrewProgressionTypes.Started, "Level", "", GameState.Progress.Level + 1);
  #endif
    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, string.Format("Level_{0:D3}", GameState.Progress.Level + 1));
  }
  void on_level_restarted(Level level)
  {
    LionAnalytics.LevelRestart(GameState.Progress.Level + 1, 0);
  }
  void on_level_finished(Level level)
  {
    if(level.Succeed)
    {
      LionAnalytics.LevelComplete(GameState.Progress.Level + 1, 0);
    #if HAS_BYTEBREW_SDK
      ByteBrew.NewProgressionEvent(ByteBrewProgressionTypes.Completed, "Level", "", GameState.Progress.Level + 1);
    #endif      
      GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, string.Format("Level_{0:D3}", GameState.Progress.Level + 1));
    }
    else
    {
      LionAnalytics.LevelFail(GameState.Progress.Level + 1, 0);
    #if HAS_BYTEBREW_SDK
      ByteBrew.NewProgressionEvent(ByteBrewProgressionTypes.Failed, "Level", "", GameState.Progress.Level + 1);
    #endif  
      GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, string.Format("Level_{0:D3}", GameState.Progress.Level + 1));
    }
  }  
  void on_invoke_rewarded_ad(string placement)
  {
    // LionAnalytics.RewardVideoShow(placement);
    // var dict = new Dictionary<string, object>()
    // {
    //   {"Placement", placement}
    // };
    // GameAnalyticsSDK.GameAnalytics.NewDesignEvent("RewardedAdShow_" + placement, dict);
  }
  void on_show_rewarded_ad(string placement)
  {
    // LionAnalytics.RewardVideoStart(placement);
    // var dict = new Dictionary<string, object>()
    // {
    //   {"Placement", placement}
    // };
    // GameAnalyticsSDK.GameAnalytics.NewDesignEvent("RewardedAdStart_" + placement, dict);
  }
  void on_show_inters_ad(string placement)
  {
    // var dict = new Dictionary<string, object>()
    // {
    //   {"Placement", placement}
    // };
    // GameAnalyticsSDK.GameAnalytics.NewDesignEvent("IntersAdStart_" + placement, dict);
  }
}
#endif
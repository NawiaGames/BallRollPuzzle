using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GOOGLE_PLAY_SERVICES

#if UNITY_ANDROID
using Google.Play.Review;
#endif

public class StoreReviewManager : MonoBehaviour
{
    [SerializeField] float firstActivationTime = 30f;
    [SerializeField] int requestInterval = 3;
    int requestCounter = 0;
    float reviewCallMinTime = 0f;

    private void Start() {
        reviewCallMinTime = Time.time + firstActivationTime;
        UISummary.onShow += RequestReview;
    }
    private void OnDestroy() {
        UISummary.onShow -= RequestReview;
    }
    public void RequestReview() //object sender) 
    {
        if (Time.time < reviewCallMinTime) { Debug.Log("Store Review | Call too early"); return; }
        if (++requestCounter % requestInterval != 0) { Debug.Log("Store Review | Not nth game | " + requestCounter + "/" + requestInterval);  return; }
        
#if UNITY_ANDROID
            StartCoroutine(PrepareReview());
#endif

#if UNITY_IOS
            UnityEngine.iOS.Device.RequestStoreReview();
#endif

#if UNITY_EDITOR
            Debug.Log("Store Review | Requested");
#endif
    }
#if UNITY_ANDROID
        IEnumerator PrepareReview(){
            var _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;

            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
                yield break;

            var _playReviewInfo = requestFlowOperation.GetResult();

            yield return null;

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;

            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
                yield break;
            
            Debug.Log("Store Review | Game Reviewed!");

            yield return new WaitForSeconds(1);
            Destroy(this);
        }
#endif
}
#endif
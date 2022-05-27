using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
public class QualityPresetSelector : MonoBehaviour
{
    [SerializeField] float delay = 5f;
    [SerializeField] float fpsThreshold = 40f;
    int frameCounter = 0;
    string qualitySaveID = "qualityPreset";

    private void Start() {
        var qualityPresetID = PlayerPrefs.GetInt(qualitySaveID, -1);
        if (qualityPresetID > 0){
            QualitySettings.SetQualityLevel(qualityPresetID);
            Destroy(this);
            return;
        }
        this.InvokeWithDelayRealTime(() => SetRenderPreset(), delay);
    }
    private void Update() {
        frameCounter++;
    }
    void SetRenderPreset(){
        var detectedFps = frameCounter/delay;
        var presetID =  detectedFps > fpsThreshold ? 0 : 1;
        PlayerPrefs.SetInt(qualitySaveID, presetID);
        QualitySettings.SetQualityLevel(presetID);
        Debug.Log("Auto Quality Preset Set! | " + detectedFps + "FPS | " + (presetID == 0 ? "Quality" : "Performance"));
        PlayerPrefs.Save();
        Destroy(this);
    }

}

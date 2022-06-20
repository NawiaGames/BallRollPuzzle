using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;

public class QualityPresetSelectorBallRollPuzzle : MonoBehaviour
{
    private void Awake() {
        GameLib.QualityPresetSelector.onVideoQualityPresetSet += SetGameSettings;
    }
    private void OnDestroy() {
        GameLib.QualityPresetSelector.onVideoQualityPresetSet -= SetGameSettings;        
    }

    void SetGameSettings(GameLib.QualityPresetSelector.VideoQualityPresets preset)
    {
        Debug.Log("Custom Settings");
    }
}

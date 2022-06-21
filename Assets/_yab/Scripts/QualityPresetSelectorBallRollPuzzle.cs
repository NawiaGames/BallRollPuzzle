using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;

public class QualityPresetSelectorBallRollPuzzle : MonoBehaviour
{
    private void Awake() {
        GameLib.GameAutoSetup.onVideoQualityPresetSet += SetGameSettings;
    }
    private void OnDestroy() {
        GameLib.GameAutoSetup.onVideoQualityPresetSet -= SetGameSettings;        
    }

    void SetGameSettings(GameLib.Defaults.VideoQualityPresets preset)
    {
        Debug.Log("Custom Settings");
    }
}

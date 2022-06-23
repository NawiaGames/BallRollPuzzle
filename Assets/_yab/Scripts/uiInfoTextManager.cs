using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.UI;

public class uiInfoTextManager : UIInfoLabelManager
{
    private void OnEnable() {
        
    }
    private void Update() {
        if(Input.GetKeyDown(KeyCode.B)) ShowTextPopup(Vector3.zero, "awesome!");
    }
}

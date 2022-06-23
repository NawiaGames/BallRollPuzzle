using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.UI;

public class UITutPwrInfo : MonoBehaviour
{
  [SerializeField] UIPanel panel;
  
  public void Activate() 
  {
    panel.ActivatePanel(); 
  }
  public void Deactivate() 
  {
    if(panel.IsActive)
      panel.DeactivatePanel();
  }

  public void Hide()
  {
    Deactivate();
  }
  public void OnBtnClick()
  {
    Hide();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.UI;

public class UITutPwrInfo : MonoBehaviour
{
  [SerializeField] UIPanel panel;
  [SerializeField] UIPanel btnPanel;
  
  public void Activate() 
  {
    panel.ActivatePanel(); 
    btnPanel.ActivatePanel();
  }
  public void Deactivate() 
  {
    panel.DeactivatePanel();
    //btnPanel.DeactivatePanel();
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

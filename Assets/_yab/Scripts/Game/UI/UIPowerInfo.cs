using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.UI;

public class UIPowerInfo : MonoBehaviour
{
  [SerializeField] UIPanel panel;
  [SerializeField] UIPanel btnPanel;

  public static System.Action<UIPowerInfo> onClosed;

  public GameState.Powerups.Type type {get; set;} = GameState.Powerups.Type.None;

  void Awake()
  {
  }
  void OnDestroy()
  {
  }

  public void Activate()
  {
    panel.ActivatePanel();
    btnPanel?.ActivatePanel();
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
    onClosed?.Invoke(this);
    Hide();
  }
}

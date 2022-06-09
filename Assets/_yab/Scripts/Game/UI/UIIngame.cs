using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIIngame : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] GameObject speedPanelContainer;
  [SerializeField] TMPLbl lblSpeed;
  [SerializeField] Slider sliderSpeed;

  [Header("TopPanelRefs")]
  [SerializeField] TMPLbl  lblLevelInfo;
  [SerializeField] TMPLbl  lblCash;
  [SerializeField] TMPLbl  lblBallsLeft;

  [Header("powerupsRefs")]
  [SerializeField] UIPanel  bottomPanel;
  [SerializeField] UIPowerupBtn[] powerups;

  public static System.Action<int, bool> onPowerupChanged;

  void Awake()
  {
    GetComponent<UIPanel>().ActivatePanel();

    Level.onCreate += OnLevelStart;
    Level.onItemThrow += OnItemThrow;
  }
  void OnDestroy()
  {
    Level.onCreate -= OnLevelStart;
    Level.onItemThrow -= OnItemThrow;
  }
  void OnLevelStart(Level lvl)
  {
    lblLevelInfo.text = "level: " + lvl.LevelIdx + "\n";
    lblBallsLeft.text = lvl.movesAvail.ToString();

    PowerupsDeselect();
    bottomPanel.ActivatePanel();
  }
  void OnItemThrow(Level lvl)
  {
    lblBallsLeft.text = lvl.movesAvail.ToString();
    PowerupsDeselect();
  }
  public void OnBtnRestart()
  {
    FindObjectOfType<Game>()?.RestartLevel();
  }
  void PowerupsDeselect()
  {
    System.Array.ForEach(powerups, (UIPowerupBtn btn) => {btn.IsSelected = false;});
  }
  void PowerupChangeSel(int idx)
  {
    System.Array.ForEach(powerups, (UIPowerupBtn btn) => { if(btn != powerups[idx]) btn.IsSelected = false; });
    powerups[idx].IsSelected = !powerups[idx].IsSelected;
    onPowerupChanged?.Invoke(idx, powerups[idx].IsSelected);
  }
  public void OnBtnPowerup0()
  {
    PowerupChangeSel(0);
  }
  public void OnBtnPowerup1()
  {
    PowerupChangeSel(1);
  }
  public void OnBtnPowerup2()
  {
    PowerupChangeSel(2);
  }
  // public void Show(Level level)
  // {
  //   GetComponent<UIPanel>().ActivatePanel();
  //   bottomPanel.ActivatePanel();
  // }
  // void Hide()
  // {
  //   //GetComponent<UIPanel>().DeactivatePanel();
  //   bottomPanel.DeactivatePanel();
  // }
}

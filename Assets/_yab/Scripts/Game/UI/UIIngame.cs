using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIIngame : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] Slider progress;
  [SerializeField] TMPLbl score;

  [Header("TopPanelRefs")]
  [SerializeField] TMPLbl  lblLevelInfo;
  [SerializeField] TMPLbl  lblCash;
  [SerializeField] TMPLbl  lblBallsLeft;

  [Header("powerupsRefs")]
  [SerializeField] UIPanel  bottomPanel;
  [SerializeField] UIPowerupBtn[] powerups;

  [SerializeField] UIPanel  comboPanel;
  [SerializeField] TMPLbl   comboText;
  [SerializeField] float    comboAnimDuration;

  public static System.Action<int, bool> onPowerupChanged;

  int _pointDest = 0;

  void Awake()
  {
    GetComponent<UIPanel>().ActivatePanel();

    Level.onCreate += OnLevelStart;
    Level.onItemThrow += OnItemThrow;
    Level.onPointsAdded += OnPointsAdded;
    Level.onCombo += OnCombo;
  }
  void OnDestroy()
  {
    Level.onCreate -= OnLevelStart;
    Level.onItemThrow -= OnItemThrow;
    Level.onPointsAdded -= OnPointsAdded;
    Level.onCombo -= OnCombo;
  }
  void OnLevelStart(Level lvl)
  {
    lblLevelInfo.text = "level: " + lvl.LevelIdx + "\n";
    lblBallsLeft.text = lvl.movesAvail.ToString();

    progress.minValue = 0;
    progress.value = 0;
    progress.maxValue = lvl.PointsMax;
    _pointDest = 0;
    UpdateScore();


    PowerupsDeselect();
    bottomPanel.ActivatePanel();
  }
  void UpdateScore()
  {
    score.text = "Score: " + (int)progress.value;
  }
  void OnItemThrow(Level lvl)
  {
    lblBallsLeft.text = lvl.movesAvail.ToString();
    PowerupsDeselect();
  }
  void OnPointsAdded(Level lvl)
  {
    _pointDest = lvl.Points;
  }
  void OnCombo()
  {
    comboPanel.ActivatePanel();
    this.Invoke(() => comboPanel.DeactivatePanel(), comboAnimDuration);
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

  void Update()
  {
    if(progress.value != _pointDest)
    {
      progress.value = Mathf.MoveTowards(progress.value, _pointDest, Time.deltaTime * 200.0f);
      UpdateScore();
    }
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

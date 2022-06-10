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
  [SerializeField] TMPLbl  lblBallsCnt;
  [SerializeField] TMPLbl  lblCash;
  [SerializeField] TMPLbl  lblBallsLeft;

  [Header("powerupsRefs")]
  [SerializeField] UIPanel  bottomPanel;
  [SerializeField] UIPowerupBtn[] powerups;

  [SerializeField] UIPanel  comboPanel;
  [SerializeField] TMPLbl   comboText;
  [SerializeField] float    comboAnimDuration;

  public static System.Action<int, bool> onPowerupChanged;

  int   _pointDest = 0;
  float _pointCurr = 0;
  int   _maxBalls = 0;

  Level _lvl = null;

  void Awake()
  {
    GetComponent<UIPanel>().ActivatePanel();

    Level.onCreate += OnLevelStart;
    Level.onItemThrow += OnItemThrow;
    Level.onPointsAdded += OnPointsAdded;
    Level.onDestroy += OnLevelDestroy;
    Level.onCreate += OnLevelCreated;
    Level.onCombo += OnCombo;
    Item.onHide += UpdateBallsInfo;
  }
  void OnDestroy()
  {
    Level.onCreate -= OnLevelStart;
    Level.onItemThrow -= OnItemThrow;
    Level.onPointsAdded -= OnPointsAdded;
    Level.onDestroy -= OnLevelDestroy;
    Level.onCreate -= OnLevelCreated;
    Level.onCombo -= OnCombo;
    Item.onHide -= UpdateBallsInfo;
  }
  void OnLevelCreated(Level lvl)
  {
    _lvl = lvl;
  }
  void OnLevelStart(Level lvl)
  {
    _lvl = lvl;
    lblLevelInfo.text = "level: " + lvl.LevelIdx + "\n";
    lblBallsLeft.text = lvl.movesAvail.ToString();

    progress.minValue = 0;
    progress.value = 0;
    progress.maxValue = lvl.PointsMax;
    _pointCurr = 0;
    _pointDest = 0;
    UpdateScore();
    UpdateBallsInfo(null);

    PowerupsDeselect();
    bottomPanel.ActivatePanel();
  }
  public void SetLevel(Level lvl)
  {
    _lvl = lvl;
  }
  void UpdateScore()
  {
    score.text = "Score: " + (int)_pointCurr;
  }
  void UpdateBallsInfo(Item sender)
  {
    if(_lvl)
      lblBallsCnt.text = "" + (_lvl.BallsInitialCnt-_lvl.ColorItems) + "/" + _lvl.BallsInitialCnt;
  }
  void OnLevelDestroy(Level lvl)
  {
    _lvl = null;
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
    if(_pointCurr != _pointDest)
    {
      _pointCurr = Mathf.MoveTowards(_pointCurr, _pointDest, Time.deltaTime * 500.0f);
      progress.value = _pointCurr;
      UpdateScore();
    }
    UpdateBallsInfo(null);
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

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
  [SerializeField] UIPanel topPanel;
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

  public static System.Action<GameState.Powerups.Type, bool> onPowerupChanged;

  int   _pointDest = 0;
  float _pointCurr = 0;
  int   _maxBalls = 0;

  Level _lvl = null;

  void Awake()
  {
    Level.onCreate += OnLevelStart;
    Level.onFinished += OnLevelFinished;
    Level.onItemThrow += OnItemThrow;
    Level.onPowerupUsed += OnPowerupUsed;
    Level.onPointsAdded += OnPointsAdded;
    Level.onMovesLeftChanged += OnMovesLeftChanged;
    Level.onDestroy += OnLevelDestroy;
    Level.onCombo += OnCombo;
    Item.onHide += OnItemHide;
  }
  void OnDestroy()
  {
    Level.onCreate -= OnLevelStart;
    Level.onFinished -= OnLevelFinished;
    Level.onItemThrow -= OnItemThrow;
    Level.onPowerupUsed -= OnPowerupUsed;
    Level.onPointsAdded -= OnPointsAdded;
    Level.onMovesLeftChanged -= OnMovesLeftChanged;
    Level.onDestroy -= OnLevelDestroy;
    Level.onCombo -= OnCombo;
    Item.onHide -= OnItemHide;
  }


  public void Show(Level level)
  {
    GetComponent<UIPanel>()?.ActivatePanel();
    topPanel.ActivatePanel();
    if(GameState.Powerups.PowerupsToShow())
    {
      SetupPowerupsInfo();
      bottomPanel.ActivatePanel();
    }
  }
  void Hide()
  {
    topPanel.DeactivatePanel();
    bottomPanel.DeactivatePanel();
  }
  void SetupPowerupsInfo()
  {
    for(int q = 0; q < (int)GameState.Powerups.Type.Cnt; ++q)
    {
      int lvl = GameData.Rewards.GetLevelForPowerup((GameState.Powerups.Type)q);
      powerups[q].SetLevel(lvl);
    }
  }
  void OnLevelCreated(Level lvl)
  {
    _lvl = lvl;
    OnLevelStart(lvl);
  }
  void OnLevelStart(Level lvl)
  {
    _lvl = lvl;
    lblLevelInfo.text = "Level " + (lvl.LevelIdx + 1);
    lblBallsLeft.text = lvl.movesAvail.ToString();

    progress.minValue = 0;
    progress.value = 0;
    progress.maxValue = lvl.PointsMax;
    _pointCurr = 0;
    _pointDest = 0;
    UpdateScore();
    UpdateBallsInfo(null);

    PowerupsDeselect();
    Show(lvl);
  }
  void OnLevelFinished(Level lvl)
  {
    Hide();
  }
  public void SetLevel(Level lvl)
  {
    _lvl = lvl;
  }
  void UpdateScore()
  {
    score.text = "Score " + (int)_pointCurr;
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
  void OnItemHide(Item sender)
  {
    this.Invoke(()=>UpdateBallsInfo(null), 0.5f);
  }
  void OnItemThrow(Level lvl)
  {
    lblBallsLeft.text = (lvl.movesAvail).ToString();
    PowerupsDeselect();
    UpdateBallsInfo(null);
  }
  void OnPowerupUsed(GameState.Powerups.Type type)
  {
    PowerupsDeselect();
    GameState.Powerups.Used(type);
    PowerupsUpdate();
  }
  void OnPointsAdded(Level lvl)
  {
    _pointDest = lvl.Points;
  }
  void OnMovesLeftChanged(Level lvl)
  {
    lblBallsLeft.text = (lvl.movesAvail).ToString();
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
  void PowerupsUpdate()
  {
    for(int q = 0; q < powerups.Length; ++q)
    {
      powerups[q].SetState(GameState.Progress.LevelPlayed > GameData.Rewards.GetLevelForPowerup((GameState.Powerups.Type)q));
      powerups[q].SetLevel(GameData.Rewards.GetLevelForPowerup((GameState.Powerups.Type)q));
      powerups[q].SetCount(GameState.Powerups.GetCount((GameState.Powerups.Type)q));
    }
  }
  void PowerupsDeselect()
  {
    System.Array.ForEach(powerups, (UIPowerupBtn btn) => {btn.IsSelected = false;});
    PowerupsUpdate();
  }
  int  GetPowerupIdx(GameState.Powerups.Type type) => System.Array.FindIndex(powerups, (btn) => btn.type == type);
  void PowerupChangeSel(GameState.Powerups.Type type)
  {
    System.Array.ForEach(powerups, (UIPowerupBtn btn) => { if(btn.type != type) btn.IsSelected = false; });
    if(GameState.Powerups.GetCount(type) > 0)
    {
      int idx = GetPowerupIdx(type);
      if(idx >= 0)
      {
        powerups[idx].IsSelected = !powerups[idx].IsSelected;
        onPowerupChanged?.Invoke(type, powerups[idx].IsSelected);
      }
    }
    PowerupsUpdate();
  }
  public void OnBtnPowerup(UIPowerupBtn sender)
  {
    PowerupChangeSel(sender.type);
  }

  void Update()
  {
    if(_pointCurr != _pointDest)
    {
      _pointCurr = Mathf.MoveTowards(_pointCurr, _pointDest, Time.deltaTime * 1200);
      progress.value = _pointCurr;
      UpdateScore();
    }
    UpdateBallsInfo(null);
  }
}

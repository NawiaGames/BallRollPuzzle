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
  [SerializeField] UIPanel    topPanel;
  [SerializeField] TMPLbl     lblLevelInfo;
  [SerializeField] TMPLbl     lblBallsCnt;
  [SerializeField] TMPLbl     lblBallsMax;
  [SerializeField] TMPLbl     lblCash;
  [SerializeField] TMPLbl     lblBallsLeft;
  [SerializeField] GameObject moveLblObj;

  [Header("PowerupsRefs")]
  [SerializeField] UIPanel  bottomPanel;
  [SerializeField] UIPowerupBtn[] powerups;

  [Header("ComboRefs")]
  [SerializeField] UIPanel  comboPanel;
  [SerializeField] TMPLbl   comboText;
  [SerializeField] float    comboAnimDuration;

  [SerializeField] UIPanel  tutorialMoves;

  [Header("DevSettings")]
  [SerializeField] bool _dontShowUI = false;

  public static System.Action<GameState.Powerups.Type, bool> onPowerupChanged;
  public static System.Action<GameObject> onMoveLblChanged;

  int   _pointDest = 0;
  float _pointCurr = 0;

  Level _lvl = null;

  static UIIngame static_this = null;
  public static bool DontShowUI => static_this?._dontShowUI ?? false;

#if UNITY_EDITOR
  UIIngame()
  {
    static_this = this;
  }
#endif  

  void Awake()
  {
    Level.onCreate += OnLevelStart;
    Level.onFinished += OnLevelFinished;
    Level.onItemThrow += OnItemThrow;
    Level.onPowerupUsed += OnPowerupUsed;
    Level.onPointsAdded += OnPointsAdded;
    Level.onMovesLeftChanged += OnMovesLeftChanged;
    Level.onTutorialStart += OnTutorialStart;
    Level.onDestroy += OnLevelDestroy;
    Level.onCombo += OnCombo;
    Item.onHide += OnItemHide;
    UIPowerupBtn.onClicked += OnBtnPowerup;
  }
  void OnDestroy()
  {
    Level.onCreate -= OnLevelStart;
    Level.onFinished -= OnLevelFinished;
    Level.onItemThrow -= OnItemThrow;
    Level.onPowerupUsed -= OnPowerupUsed;
    Level.onPointsAdded -= OnPointsAdded;
    Level.onMovesLeftChanged -= OnMovesLeftChanged;
    Level.onTutorialStart -= OnTutorialStart;
    Level.onDestroy -= OnLevelDestroy;
    Level.onCombo -= OnCombo;
    Item.onHide -= OnItemHide;
    UIPowerupBtn.onClicked -= OnBtnPowerup;
  }


  public void Show(Level level)
  {
    if(_dontShowUI)
      return;
    GetComponent<UIPanel>()?.ActivatePanel();
    topPanel.ActivatePanel();
    if(GameState.Powerups.PowerupsToShow())
    {
      SetupPowerupsInfo();
      //PowerupsUpdate();
      bottomPanel.ActivatePanel();
    }
  }
  void Hide()
  {
    if(_dontShowUI)
      return;    
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
    this.Invoke(() => 
    {
      var type = GameState.Powerups.TutorialToShow();
      if(type != GameState.Powerups.Type.None)
      {
        int idx = GetPowerupIdx(type);
        if(idx >= 0)
          powerups[idx].ShowTut(true);
      }
    },1.0f);
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
    UpdateMovesLeft(_lvl);

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
    UpdateBallsInfo(null);
  }
  void UpdateBallsInfo(Item sender)
  {
    if(_lvl)
    {
      lblBallsCnt.text = "" + (_lvl.BallsInitialCnt-_lvl.ColorItems);
      lblBallsMax.text = "of" + _lvl.BallsInitialCnt;
    }
  }
  void UpdateMovesLeft(Level lvl)
  {
    if(lvl.tutorial == Level.Tutorial.Push)
      lblBallsLeft.text = "-";
    else
      lblBallsLeft.text = lvl.movesAvail.ToString();
  }
  void OnLevelDestroy(Level lvl)
  {
    _lvl = null;
  }
  void OnItemHide(Item sender)
  {
    this.Invoke(()=>UpdateBallsInfo(null), -3);
  }
  void OnItemThrow(Level lvl)
  {
    UpdateMovesLeft(lvl);
    PowerupsDeselect();
    UpdateBallsInfo(null);
    if(lvl.tutorial == Level.Tutorial.Moves)
      tutorialMoves.DeactivatePanel();    
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
    UpdateMovesLeft(lvl);
    onMoveLblChanged?.Invoke(moveLblObj);
  }
  void OnTutorialStart(Level lvl)
  {
    if(lvl.tutorial == Level.Tutorial.Moves)
      tutorialMoves.ActivatePanel();
  }
  void OnCombo(int events_cnt)
  {
    comboPanel.ActivatePanel();
    if(events_cnt == 2)
      comboText.text = GameData.Points.randomComboText0;
    else if(events_cnt > 2)
      comboText.text = GameData.Points.randomComboText1;
    comboText.GetComponent<GameLib.Utilities.TextEffects.TextAnimator>().RefreshTextAnimatorData();  
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
        powerups[idx].ShowInfo(powerups[idx].IsSelected);
        if(!powerups[idx].IsSelected)
          powerups[idx].ShowTut(false);
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
  }
}

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
  [SerializeField] TMPLbl  lblCustomers;

  void Awake()
  {
    GetComponent<UIPanel>().ActivatePanel();
    Level.onStart += OnLevelStart;
  }
  void OnDestroy()
  {
    Level.onStart -= OnLevelStart;
  }
  void OnLevelStart(Level lvl)
  {
    lblLevelInfo.text = "level: " + lvl.LevelIdx + "\n";
    //lblLevelInfo.text += "gameType: " + ((lvl.gameType == Level.GameType.Match3Move)? "Match3 + move" : "Match3") + "\n";
    // string push = "";
    // if(lvl.pushType == Level.PushType.None)
    //   push = "No push - new balls";
    // else if(lvl.pushType == Level.PushType.PushOne)
    //   push = "push to next field";
    // else if(lvl.pushType == Level.PushType.PushLine)
    //   push = "push to obstacle";
    // lblLevelInfo.text += "pushType: " + push + "\n";
    //lblLevelInfo.text += "push outside gamefield: " + (lvl.gameOutside).ToString();
  }
  public void OnBtnRestart()
  {
    FindObjectOfType<Game>()?.RestartLevel();
  }
  public void Show(Level level)
  {
    GetComponent<UIPanel>().ActivatePanel();
  }
  void Hide()
  {
    GetComponent<UIPanel>().ActivatePanel();
  }
}

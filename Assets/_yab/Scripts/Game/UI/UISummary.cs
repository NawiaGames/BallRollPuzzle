using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLib.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UISummary : MonoBehaviour
{
  public static System.Action onShow, onBtnPlay;

  [SerializeField] UIPanel navNextPanel;
  [SerializeField] UIPanel navRestartPanel;

  [SerializeField] GameObject winContainer;
  [SerializeField] GameObject failContainer;
  //[SerializeField] TMPLbl  lblScore;
  //[SerializeField] TMPLbl  lblInfo;
  [SerializeField] Slider  slider;
  [SerializeField] UITwoState[] stars;
  [SerializeField] TMPLbl  scores;
  [SerializeField] string  strScoresFmt = "score: {0}";

  float destValue = 0;
  public void Show(Level level)
  {
    winContainer.SetActive(level.Succeed);
    failContainer.SetActive(!level.Succeed);
    GetComponent<UIPanel>().ActivatePanel();
    if(level.Succeed)
      navNextPanel.ActivatePanel();
    else
      navRestartPanel.ActivatePanel();

    slider.minValue = 0;
    slider.maxValue = 0;
    slider.value = 0;
    destValue = 0;
    scores.text = string.Format(strScoresFmt, level.Points.ToString());

    for(int q = 0; q < stars.Length; ++q)
    {
      stars[q].SetState(level.Stars > q);
    }

    onShow?.Invoke();

    AddRewards(level);

    this.Invoke(()=> destValue = 0, 1.0f);
  }
  void Hide()
  {
    GetComponent<UIPanel>().DeactivatePanel();
  }
  void AddRewards(Level level)
  {
    var reward = GameData.Rewards.GetReward(level.LevelIdx);
    if(reward != null)
      GameState.Powerups.AddReward(reward);
  }
  public void OnBtnRestart()
  {
    Hide();
    FindObjectOfType<Game>().RestartLevel();
  }
  public void OnBtnPlay()
  {
    Hide();
    FindObjectOfType<Game>().NextLevel();
    onBtnPlay?.Invoke();
  }
  void Update()
  {
    //slider.value = Mathf.MoveTowards(slider.value, destValue, Time.deltaTime * 4);
  }
}

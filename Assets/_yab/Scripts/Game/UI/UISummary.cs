using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLib.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UISummary : MonoBehaviour
{
  public static System.Action onShow, onBtnPlay;

  [SerializeField] UIPanel winContainer;
  [SerializeField] UIPanel navNextPanel;
  [SerializeField] UIPanel navWinRestartPanel;
  [SerializeField] UIPanel failContainer;
  [SerializeField] UIPanel navRestartPanel;
  [SerializeField] UIPanel rewardContainer;
  [SerializeField] UIPanel navClaimPanel;

  [SerializeField] Slider  slider;
  [SerializeField] UITwoState[] stars;
  [SerializeField] TMPLbl  levelInfo;
  [SerializeField] TMPLbl  scores;
  [SerializeField] string  strScoresFmt = "score: {0}";

  bool  updateSlider = false;
  float destValue = 0;
  bool  showReward = false;

  UIReward[] _uiRewards;

  public void Show(Level level)
  {
    levelInfo.text = "Level " + (level.LevelIdx + 1);

    var range = GameData.Rewards.GetRewardProgress();
    slider.minValue = range.beg;
    slider.maxValue = range.end+1;
    slider.value = Mathf.Max(level.LevelIdx, range.beg);
    destValue = level.LevelIdx+1;
    scores.text = string.Format(strScoresFmt, level.Points.ToString());

    updateSlider = false;
    for(int q = 0; q < stars.Length; ++q)
      stars[q].SetState(level.Stars > q);

    onShow?.Invoke();

    if(_uiRewards == null)
      _uiRewards = rewardContainer.GetComponentsInChildren<UIReward>(true);
    showReward = (level.LevelIdx == range.end && GameData.Rewards.ToClaim(level.LevelIdx));

    winContainer.gameObject.SetActive(level.Succeed);
    failContainer.gameObject.SetActive(!level.Succeed);
    GetComponent<UIPanel>().ActivatePanel();
    if(level.Succeed)
    {
      winContainer.ActivatePanel();
      StartCoroutine(Sequence(level));
    }
    else
    {
      failContainer.ActivatePanel();
      navRestartPanel.ActivatePanel();   
    }
  }
  void Hide()
  {
    updateSlider = false;
    GetComponent<UIPanel>().DeactivatePanel();
  }
  void AddRewards(Level level)
  {
    var reward = GameData.Rewards.GetReward(level.LevelIdx);
    if(reward != null)
      GameState.Powerups.AddReward(reward);
  }
  IEnumerator Sequence(Level level)
  {
    yield return new WaitForSeconds(1.0f);
    updateSlider = true;
    yield return new WaitForSeconds((showReward)?1.0f : 0.25f);
    if(level.Succeed)
    {
      if(!showReward)
      {
        navNextPanel.ActivatePanel();
        //yield return new WaitForSeconds(0.25f);
        navWinRestartPanel.ActivatePanel();
      }
      else
      {
        rewardContainer.gameObject.SetActive(level.Succeed);
        var rewards = GameData.Rewards.GetReward(level.LevelIdx);
        _uiRewards[0].Quantity((rewards.HasValue)?rewards.Value.powerupArrows : 0);
        _uiRewards[1].Quantity((rewards.HasValue)?rewards.Value.powerupBombs : 0);
        _uiRewards[2].Quantity((rewards.HasValue)?rewards.Value.powerupColors : 0);
        _uiRewards[3].Quantity((rewards.HasValue)?rewards.Value.powerupPainters : 0);
        //winContainer.SwitchPanel(rewardContainer);
        rewardContainer.SwitchPanel();
        navClaimPanel.ActivatePanel();
      }
    }
    else
      navRestartPanel.ActivatePanel();

    AddRewards(level);
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
    if(updateSlider)
      slider.value = Mathf.MoveTowards(slider.value, destValue, Time.deltaTime);
  }
}

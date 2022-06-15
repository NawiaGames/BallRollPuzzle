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
  [SerializeField] UIPanel navClaimPanel;

  [SerializeField] GameObject winContainer;
  [SerializeField] GameObject failContainer;
  [SerializeField] GameObject rewardContainer;
  //[SerializeField] TMPLbl  lblScore;
  //[SerializeField] TMPLbl  lblInfo;
  [SerializeField] Slider  slider;
  [SerializeField] UITwoState[] stars;
  [SerializeField] TMPLbl  scores;
  [SerializeField] string  strScoresFmt = "score: {0}";

  bool  updateSlider = false;
  float destValue = 0;
  bool  showReward = false;
  public void Show(Level level)
  {
    var range = GameData.Rewards.GetRewardProgress();

    slider.minValue = range.beg;
    slider.maxValue = range.end;
    slider.value = Mathf.Max(level.LevelIdx, range.beg);
    destValue = level.LevelIdx+1;
    scores.text = string.Format(strScoresFmt, level.Points.ToString());

    updateSlider = false;
    for(int q = 0; q < stars.Length; ++q)
      stars[q].SetState(level.Stars > q);

    onShow?.Invoke();
    
    showReward = level.LevelIdx >= range.end;

    winContainer.SetActive(level.Succeed);
    failContainer.SetActive(!level.Succeed);
    GetComponent<UIPanel>().ActivatePanel();

    if(level.Succeed)
    {
      winContainer.GetComponent<UIPanel>().ActivatePanel();
      StartCoroutine(Sequence(level));
    }
    else
      navRestartPanel.ActivatePanel();   

    // if(level.Succeed)
    // {
    //   if(!showReward)
    //     navNextPanel.ActivatePanel();
    //   else
    //     navClaimPanel.ActivatePanel();  
    // }
    // else
    //   navRestartPanel.ActivatePanel();

    // AddRewards(level);

    // this.Invoke(()=> updateSlider = true, 1.0f);
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
    yield return new WaitForSeconds(2.0f);
    updateSlider = true;
    yield return new WaitForSeconds(0.25f);
    if(level.Succeed)
    {
      if(!showReward)
        navNextPanel.ActivatePanel();
      else
      {
        rewardContainer.SetActive(level.Succeed);
        winContainer.GetComponent<UIPanel>().SwitchPanel(rewardContainer.GetComponent<UIPanel>());
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
      slider.value = Mathf.MoveTowards(slider.value, destValue, Time.deltaTime * 4);
  }
}

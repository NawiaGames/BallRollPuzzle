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

    onShow?.Invoke();

    this.Invoke(()=> destValue = 0, 1.0f);
  }
  void Hide()
  {
    GetComponent<UIPanel>().DeactivatePanel();
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
    slider.value = Mathf.MoveTowards(slider.value, destValue, Time.deltaTime * 4);
  }
}

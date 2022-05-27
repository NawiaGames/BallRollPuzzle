using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIStart : MonoBehaviour
{
  public static System.Action onShow, onBtnStart;

  [SerializeField] TMPLbl                levelIdx;
  [SerializeField] TMPLbl                levelInfo;

  void Awake()
  {
  }
  public void Show(Level level)
  {
    levelIdx.text = "Level " + (level.LevelIdx + 1).ToString();
    onShow?.Invoke();
  }
  void Hide()
  {
    GetComponent<UIPanel>().DeactivatePanel();
  }
  
  public void OnBtnPlay()
  {
    onBtnStart?.Invoke();
    Hide();
  }
}

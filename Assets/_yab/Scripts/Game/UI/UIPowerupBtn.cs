using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIPowerupBtn : MonoBehaviour
{
  [SerializeField] Image  ico;
  [SerializeField] TMPLbl lblOn;
  [SerializeField] TMPLbl lblOff;
  [SerializeField] GameObject stateOn, stateOff;
  [SerializeField] GameObject _selection;
  [SerializeField] float _selScale = 1.2f;
  [SerializeField] GameState.Powerups.Type _type;
  [SerializeField] UITutPwrInfo tutPowerInfo;
  [SerializeField] UIPowerInfo  powerInfo;

  float _scaleEnd = 1.0f;
  bool _selected = false;
  bool _unlocked = false;

  void Awake()
  {
    UIPowerInfo.onClosed += OnPowerupClosed;
  }
  void OnDestroy()
  {
    UIPowerInfo.onClosed -= OnPowerupClosed;
  }

  public bool IsSelected 
  {
    get => _selected;
    set 
    { 
      _selected = value; 
      _scaleEnd = (_selected)? _selScale : 1.0f; 
      _selection.SetActive(_selected);
      ShowInfo(value);
    }
  }
  public GameState.Powerups.Type type => _type;
  public void SetCount(int cnt){lblOn.text = cnt.ToString();}
  public void SetLevel(int lvl){lblOff.text = string.Format("lvl {0}", lvl+2);}
  public void SetState(bool unlocked)
  {
    stateOn.SetActive(unlocked);
    stateOff.SetActive(!unlocked);
  }
  public void ShowTut(bool show)
  {
    powerInfo.type = type;
    if(show)
      tutPowerInfo.Activate();
    else
      tutPowerInfo.Deactivate();
  }
  public void ShowInfo(bool show)
  {
    powerInfo.type = type;
    if(show)
      powerInfo.Activate();
    else
      powerInfo.Deactivate();
  }
  public void Reset()
  {
    ico.transform.localScale = Vector3.one;
    _scaleEnd = 1.0f;
  }
  void OnPowerupClosed(UIPowerInfo pwrInfo)
  {
    if(pwrInfo.type == type)
    {
      FindObjectOfType<UIIngame>().OnBtnPowerup(this);
      //IsSelected = false;
    }
  }
  void Update()
  {
    var _scale = Mathf.MoveTowards(ico.transform.localScale.x, _scaleEnd, Time.deltaTime * 2.0f);
    ico.transform.localScale = Vector3.one * _scale;
  }
}


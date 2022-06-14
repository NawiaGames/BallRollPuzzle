using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UIPowerupBtn : MonoBehaviour
{
  [SerializeField] Image  ico;
  [SerializeField] TMPLbl lblOn;
  [SerializeField] TMPLbl lblOff;
  [SerializeField] GameObject stateOn, stateOff;
  [SerializeField] float _selScale = 1.2f;
  [SerializeField] GameState.Powerups.Type _type;

  float _scaleEnd = 1.0f;
  bool _selected = false;
  bool _unlocked = false;

  public bool IsSelected 
  {
    get => _selected;
    set { _selected = value; _scaleEnd = (_selected)? _selScale : 1.0f;}
  }
  public GameState.Powerups.Type type => _type;
  public void SetCount(int cnt){lblOn.text = cnt.ToString();}
  public void SetLevel(int lvl){lblOff.text = string.Format("lvl {0}", lvl+2);}
  public void SetState(bool unlocked)
  {
    stateOn.SetActive(unlocked);
    stateOff.SetActive(!unlocked);
  }
  public void Reset()
  {
    ico.transform.localScale = Vector3.one;
    _scaleEnd = 1.0f;
  }
  void Update()
  {
    var _scale = Mathf.MoveTowards(ico.transform.localScale.x, _scaleEnd, Time.deltaTime * 2.0f);
    ico.transform.localScale = Vector3.one * _scale;
  }
}


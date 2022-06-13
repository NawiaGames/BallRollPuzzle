using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UIPowerupBtn : MonoBehaviour
{
  [SerializeField] Image ico;
  [SerializeField] TMPLbl lbl;
  [SerializeField] float _selScale = 1.2f;

  float _scaleEnd = 1.0f;
  bool _selected = false;

  public bool IsSelected 
  {
    get => _selected;
    set { _selected = value; _scaleEnd = (_selected)? _selScale : 1.0f;}
  }
  public void SetCount(int cnt){lbl.text = cnt.ToString();}
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

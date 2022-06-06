using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;

public class GridElem : MonoBehaviour
{
  [SerializeField] Renderer   rend;
  [SerializeField] GameObject _fx;
  [SerializeField] Color      colorEven;
  [SerializeField] Color      colorOdd;
  [SerializeField] ActivatableObject _actObj;
  
  bool _even = true;
  public bool even{get => _even; set{_even = value; SetColor();}}
  public Vector2Int grid {get; set;}
  bool _shown = false;


  Vector3 _vvel = Vector3.zero;
  Vector3 _vforce = Vector3.zero;
  Vector3 _vpos = Vector3.zero;
  Vector3 _voffs = Vector3.zero;

  public void Show()
  {
    if(!_shown)
      _actObj.ActivateObject();
    _shown = true;
  }
  public void Hide()
  {
    _actObj.DeactivateObject();
  }
  void SetColor()
  {
    rend.material.color = (_even)? colorEven : colorOdd;
  }
  public void Touch(float delay)
  {
    //StartCoroutine(coTouch(delay));
    _vvel += new Vector3(0, -Random.Range(0, 1), 0);
  }
  void Update()
  {
    //var _voff = _vpos - Vecto3
  }
}

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
  [SerializeField] SpriteRenderer _sr;
  [SerializeField] ActivatableObject _actObj;
  
  bool _even = true;
  public bool even{get => _even; set{_even = value; SetColor();}}
  public Vector2Int grid {get; set;}
  public bool selected {get; set;} = false;

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
    if(selected)
    {
      var color = _sr.material.color;
      color.a = Mathf.Clamp01(color.a + Time.deltaTime * 4);
      _sr.material.color = color;
    }
    else
    {
      var color = _sr.material.color;
      color.a = Mathf.Clamp01(color.a - Time.deltaTime * 4);
      _sr.material.color = color;
    }
  }
}

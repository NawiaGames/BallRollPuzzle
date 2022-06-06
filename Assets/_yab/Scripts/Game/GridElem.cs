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
  public void Touch(float velmult = 1.0f)
  {
    _vvel.y += -0.5f * velmult;
  }
  public void TouchRandom()
  {
    _vvel.y -= Random.Range(0.1f, 0.5f);
  }  
  void ProcessSelection()
  {
    var color = _sr.material.color;
    if(selected)
      color.a = Mathf.Clamp01(color.a + Time.deltaTime * 4);
    else
      color.a = Mathf.Clamp01(color.a - Time.deltaTime * 4);
    _sr.material.color = color;
  }
  void Update()
  {
    ProcessSelection();

    //if(_vvel.sqrMagnitude > 0.01f)
    {
      var _voff = _vpos;
      _vforce = -0.15f * _voff;
      _vvel += _vforce;
      _vpos += _vvel * Time.deltaTime * 4;
      _fx.transform.localPosition = _vpos;
      _vvel *= 0.95f;
    }
  }
}

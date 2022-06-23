using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;

public class GridElem : MonoBehaviour
{
  [SerializeField] Renderer   rend;
  [SerializeField] GameObject _fx;
  [SerializeField] GameObject _model;
  [SerializeField] Color      colorEven;
  [SerializeField] Color      colorOdd;
  [SerializeField] SpriteRenderer _sr;
  [SerializeField] ActivatableObject _actObj;
  [SerializeField] ObjectFracture _fracture;
  
  bool _even = true;
  Vector2Int vdir = new Vector2Int(0, 1);
  public bool even{get => _even; set{_even = value; SetColor();}}
  public Vector2Int grid {get; set;}
  bool _selected = false;
  bool _shown = false;


  Vector3 _vvel = Vector3.zero;
  Vector3 _vforce = Vector3.zero;
  Vector3 _vpos = Vector3.zero;
  Vector3 _voffs = Vector3.zero;

  void OnDestroy()
  {
    _fracture.ResetFracture();
  }

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
  public void HideFail()
  {
    _fx.GetComponent<Animator>().SetTrigger("fail");
  }
  public void SetVis(bool vis)
  {
    rend.gameObject.SetActive(vis);
  }
  void SetColor()
  {
    rend.material.color = (_even)? colorEven : colorOdd;
  }
  public void Fracture()
  {
    rend.gameObject.SetActive(false);
    _fracture.Fracture(Vector3.up * 4);
  }
  public void SetSelected(bool sel, Vector2Int dir)
  {
    if(sel)
    {
      vdir = dir;
      _sr.transform.localRotation = Quaternion.LookRotation(new Vector3(vdir.x, -90, vdir.y), Vector3.up);
    }
    _selected = sel;
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
    if(_selected)
      color.a = Mathf.Clamp01(color.a + Time.deltaTime * 4);
    else
      color.a = Mathf.Clamp01(color.a - Time.deltaTime * 4);

    _sr.material.color = color;
  }
  void Update()
  {
    ProcessSelection();

    if(_vvel.magnitude * Time.deltaTime > 0.000001f)
    {
      var _voff = _vpos;
      _vforce = -0.15f * _voff;
      _vvel += _vforce;
      _vpos += _vvel * Time.deltaTime * 4;
      _model.transform.localPosition = _vpos;
      _vvel *= 0.95f;
    }
  }
}

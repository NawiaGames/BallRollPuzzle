using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.Utilities;

public class Arrow : MonoBehaviour
{
  [SerializeField] Renderer _renderer;
  [SerializeField] Transform _fx;
  [SerializeField][ColorUsage(true, true)] Color _colorNormal;
  [SerializeField][ColorUsage(true, true)] Color _colorSelected;
  [SerializeField][ColorUsage(true, true)] Color _colorBlocked;
  [SerializeField] float blendSpeed = 4f;
  [SerializeField] ActivatableObject _actObj;
  [SerializeField] ObjectShake _shake;

  Color   targetColor = Color.white;
  bool    _selected = false;
  bool    _blocked = false;
  Vector2Int _grid = Vector2Int.zero;

  MaterialPropertyBlock materialPropertyBlock;
  Color lerpColor = Color.white;

  void Awake()
  {
    materialPropertyBlock = new MaterialPropertyBlock();
    _renderer.SetPropertyBlock(materialPropertyBlock);
    // _sr.color = _colorNormal;
    targetColor = _colorNormal;
    lerpColor = targetColor;
  }
  public void Shake() => _shake.Shake();
  public void Show()
  {
    _actObj.ActivateObject();
  }
  public void Hide()
  {
    _actObj.DeactivateObject();
  }
  public bool        IsSelected { get => _selected; set { _selected = value; targetColor = (_selected) ? _colorSelected : _colorNormal; } }
  public bool        IsBlocked { get => _blocked; set { _blocked = value; targetColor = (_blocked)? _colorBlocked : _colorNormal;}}
  public Vector3     vPos => transform.localPosition;
  public Vector2Int  grid {get; set;}
  public Vector2Int  dir {get; set;}
  public Vector3     vDir => new Vector3(dir.x, 0, dir.y);

  Vector3 _vvel = Vector3.zero;
  Vector3 _vforce = Vector3.zero;
  Vector3 _vpos = Vector3.zero;
  Vector3 _voffs = Vector3.zero;

  public  void Touch(float velmult = 1.0f)
  {
    _vvel.y += -0.5f * velmult;
  }
  private void Float()
  {
    // if(_vvel.magnitude * Time.deltaTime > 0.000001f)
    // {
    //   var _voff = _vpos;
    //   _vforce = -0.15f * _voff;
    //   _vvel += _vforce;
    //   _vpos += _vvel * Time.deltaTime * 4;
    //   _fx.transform.localPosition = _vpos;
    //   _vvel *= 0.95f;
    // }
    if(_vvel.sqrMagnitude > 0)
    {
      var _voff = _vpos;
      _vforce = -0.15f * _voff;
      _vvel += _vforce;
      _vpos += _vvel * Time.deltaTime * 4;
      _vvel *= 0.95f;
      //if(_vvel.magnitude < 0.0001f)
      if((_vvel.y > 0 && _vpos.y > 0) || _vvel.magnitude < 0.0001f)
      {
        _vvel.Set(0, 0, 0);
        _vforce.Set(0, 0, 0);
        _vpos.y = 0;
      }
      _fx.transform.localPosition = _vpos;
    }    
  }
  private void Update() 
  {
    lerpColor = Color.Lerp(lerpColor, targetColor, Time.deltaTime * blendSpeed);
    materialPropertyBlock.SetColor(Defaults.MaterialBaseColor, lerpColor);
    _renderer.SetPropertyBlock(materialPropertyBlock);

    Float();
  }
}

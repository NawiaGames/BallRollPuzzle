using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.Utilities;

public class Item : MonoBehaviour
{
  [SerializeField] int        _id = 0;
  [SerializeField] Color      _color;
  [ColorUsage(false, true)]
  [SerializeField] Color      _colorMatchPing;
  [SerializeField] GameObject _ballModel = null;
  [SerializeField] GameObject _customModel = null;

  [SerializeField] ActivatableObject _activatable;
  [SerializeField] Push       _push = Push.None;
  [SerializeField] Spec       _special = Spec.None;
  [SerializeField] bool       _autoMove = false;
  [SerializeField] float      _radius = 0.5f;
  [SerializeField] float      _startSpeed = 0.2f;
  [SerializeField] float      _accConst = 0.035f;
  [SerializeField] float      _accNonConst = 1.035f;
  [SerializeField] bool       _accTypeConst = true;
  [SerializeField] ObjectColorBlender _ocb;
  [SerializeField] ObjectShake _objShk;
  [SerializeField] float      _touchDelay = 0.5f;

  [SerializeField] Transform  _rollTransf;

  [SerializeField] ObjectShakePreset _objShakeStaticPreset;
  [SerializeField] ObjectShakePreset _objShakeBallPreset;
  [SerializeField] ObjectShakePreset _objShakeBallMovePreset;

  public enum Push
  {
    None,
    One,
    Line,
  }
  public enum Spec
  {
    None,
    Static,
    Bomb,
    ColorChange,
    RandomPush,
    RandomItem,
    RandomMoveItem,
    Painter,
    Arrows,
   
    RemoveElem = 2200,
    DirectionalItem = 3000
  }

  MeshRenderer _mr = null;
  MaterialPropertyBlock _mpb = null;

  Vector2Int _vturn = new Vector2Int(0,1);
  Vector2Int _dirLast = Vector2Int.zero;
  Vector2Int _dir = Vector2Int.zero;
  Vector2Int _gridPrev = Vector2Int.zero;
  Vector2Int _gridBeg = Vector2Int.zero;
  Vector2Int _gridEnd = Vector2Int.zero;
  float      _lifetime = 0;
  bool       _frozen = false;
  float      _circum = 2 * Mathf.PI;
  float      _speed = 0;
  bool       _hidding = false;

  public static System.Action<Item> onShow, onHide, onHit, onBombExplode, onExplode, onPushedOut, onPaint;

  static Vector2Int  toGridT(Vector3 vpos, Vector2Int vdir) => new Vector2Int((int)Mathf.Round(vpos.x - vdir.x * 0.5f), (int)Mathf.Round(vpos.z - vdir.y * 0.5f));  
  static Vector3     toPos(Vector2Int grid) => new Vector3(grid.x, 0,  grid.y);
  public static bool EqType(Item item0, Item item1)
  {
    return item0 != null && item1 != null && item0.id == item1.id;
    //return item0 != null && item1 != null && item0.name.Equals(item1.name);
  }

  void Awake()
  {
    Init();
  }
  public void Init()
  {
    if(IsRegular || IsColorChanger)
      _mr = _ballModel.GetComponent<MeshRenderer>();
    else
      _mr = GetComponentInChildren<MeshRenderer>();
    _mpb = new MaterialPropertyBlock();
    _mr.GetPropertyBlock(_mpb, 0);

    color = _color;
    transform.localPosition = transform.localPosition.round();
    grid = new Vector2Int(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.z));
    var v = transform.localRotation * new Vector3(0, 0, 1);
    _vturn.x = Mathf.RoundToInt(v.x);
    _vturn.y = Mathf.RoundToInt(v.z);
    _circum = 2 * Mathf.PI * _radius;

    name = name.Replace("(Clone)", "");
  }
  
  public int   id { get => _id; set{ _id = value;}}
  public Color color
  {
    get => _color; 
    set
    { 
      _color = value; 
      _mr.material.color = _color;
      _mpb.SetColor("_MainColor", _color);
      _mpb.SetColor("_BaseColor", _color);
      _mr.SetPropertyBlock(_mpb);
    }
  }
  public Vector2Int grid /*{get; set;}*/ = Vector2Int.zero;
  public Vector2Int gridPrev => _gridPrev;
  public Vector2Int gridNext => grid + _dir.to_units();
  public Vector2Int gridNextLast => grid + _dirLast.to_units();
  public Vector2Int dir {get => _dir; set{_dir = value;}}
  public Vector2Int vturn 
  { 
    get => _vturn; 
    set 
    { 
      _vturn = value; 
      Vector3 v3 = new Vector3(_vturn.x, 0, _vturn.y);
      transform.localRotation = Quaternion.LookRotation(v3, Vector3.up);
    }
  }
 
  public Vector3 vdir => new Vector3(dir.x, 0, dir.y);
  public Push push {get => _push; set{ _push = value;}}
  public bool IsReady => !_activatable.InTransition && _lifetime > 0.125f;
  public bool IsMoving => grid != _gridEnd || !Mathf.Approximately(Vector3.Distance(transform.position, toPos(_gridEnd)), 0);
  //public bool IsArrowVis {get => _arrow.activeSelf; set{_arrow.SetActive(value || IsStatic || IsBomb || _autoMove || _push != Item.Push.None && );}}
  public bool IsStatic => _special == Spec.Static;
  public bool IsFrozen {get =>_frozen; set{ _frozen = value;}}
  public bool IsBomb => _special == Spec.Bomb;
  public bool IsColorChanger => _special == Spec.ColorChange;
  public bool IsRegular => _special == Spec.None;
  public bool IsSpecial => _special != Spec.None;
  public bool IsMoveable => _autoMove;
  public bool IsRandItem => _special == Spec.RandomItem;
  public bool IsRandMoveItem => _special == Spec.RandomMoveItem;
  public bool IsRandPush => _special == Spec.RandomPush;
  public bool IsRemoveElem => _special == Spec.RemoveElem;
  public bool IsMatching {get;set;} = false;
  public bool IsPainter => _special == Spec.Painter;
  public bool IsDirectional => _special == Spec.DirectionalItem;
  public Vector2Int? Redirected {get; set;}
  public int  Points {get; set;} = 0;
  public bool IsHidding => _hidding;
  public Arrow arrow => _arr;
  public Arrow arrowInitial => _arrInitial;

  private Arrow _arrInitial = null;
  private Arrow _arr = null;


  public void Show(Arrow arr)
  {
    _arr = arr;
    _arrInitial = arr;
    _gridPrev = grid;
    _gridEnd = grid;
    if(!IsRemoveElem)
    {
      _activatable.ActivateObject();
      onShow?.Invoke(this);
      if(_arr)
      {
        this.Invoke(()=>{_arr?.Touch();_arr = null;}, _touchDelay);
      }
    }
    if(IsRegular && push == Push.None)
    {
      _rollTransf.GetChild(0).localRotation = Random.rotation;
    }
  }
  public void Hide(bool silent = false)
  {
    if(!_hidding)
    {
      _hidding = true;  
      if(!silent)
        onHide?.Invoke(this);
      StartCoroutine(WaitForEnd());
    }
  }
  public void Deactivate()
  {
    _activatable.DeactivateObject();
  }
  IEnumerator WaitForEnd()
  {
    yield return null;
    while(_activatable.InTransition)
    {
      yield return null;
    }
    gameObject.SetActive(false);
  }
  void Speed()
  {
    if(_accTypeConst)
    {
      if(_speed == 0)
        _speed = _startSpeed;
      _speed = Mathf.Clamp01(_speed + _accConst);
    }
    else
    {
      if(_speed == 0)
        _speed = _startSpeed;
      _speed = Mathf.Clamp01(_speed * _accNonConst);
    }
  }
  public void ResetSpeed()
  {
    _speed = 0;
  }
  public bool MoveP(float dt)
  {
    if(!IsReady)
      return false;
    if(_arr)
    {
      _arr.Touch();
      _arr = null;
    }

    Speed();

    bool chpos = false;
    for(int q = 0; q < 2 & !chpos; ++q)
    {
      transform.localPosition += vdir * dt * 0.5f * _speed;
      Rotate(_rollTransf, dt * 0.5f * _speed);
      var grid_prev = grid;
      grid = toGridT(transform.localPosition, dir);
      chpos |= grid != grid_prev;
      if(chpos)
        _gridPrev = grid_prev;
    }
    
    return chpos;
  }
  public bool Move(float dt, Vector2Int _dim)
  {
    if(!IsReady)
      return false;

    Speed();

    bool ret = true;
    for(int q = 0; q < 2 && ret; ++q)
    {
      transform.localPosition = Vector3.MoveTowards(transform.localPosition, toPos(_gridEnd), dt * 0.5f * _speed);
      Rotate(_rollTransf, dt * 0.5f * _speed);
      _gridPrev = grid;
      grid = toGridT(transform.localPosition, _dir);
      if(Mathf.Approximately(Vector3.Distance(transform.localPosition, toPos(_gridEnd)), 0))
      {
        _dirLast = _dir;
        _dir = Vector2Int.zero;
        grid = _gridEnd;
        transform.localPosition = toPos(_gridEnd);
        _speed = 0;
        ret = false;
      }
    }

    return ret;
  }  
  public void PushBy(Vector2Int pushDir)
  {
    if(IsStatic || IsRemoveElem)
      return;
      
    _dir = pushDir.to_units();
    _gridEnd = grid + pushDir;
    _speed = 0;
  }
  public void PushTo(Vector2Int gridDest)
  {
    if(IsStatic || IsRemoveElem)
      return;

    _dir = (gridDest - grid).to_units();
    _gridEnd = gridDest;
    _speed = 0;
  }
  public void PushedOut()
  {
    onPushedOut?.Invoke(this);
  }
  public void Stop()
  {
    _dir = Vector2Int.zero;
    _gridEnd = grid;
    _speed = 0;
    transform.localPosition = toPos(grid);
  }
  public void Hit(Item item)
  {
    if(item == null)
      return;

    if(this.IsColorChanger && item.IsRegular)
    {
      id = item.id;
      _ballModel.SetActive(true);
      _customModel.SetActive(false);
      color = item.color;
      _special = Item.Spec.None;
      name = item.gameObject.name;
    }
    else if(item.IsColorChanger && this.IsRegular)
    {
      item.id = id;
      item._ballModel.SetActive(true);
      item._customModel.SetActive(false);
      item.color = color;
      item._special = Item.Spec.None;
      item.gameObject.name = name;
    }
    Shake();

    onHit?.Invoke(this);
  }
  public void Paint(Item itemSrc)
  {
    id = itemSrc.id;
    color = itemSrc.color;
    onPaint?.Invoke(this);
  }
  public void Shake()
  {
    if(IsStatic)
      _objShk?.Shake(_objShakeStaticPreset);
    // else
    //   _objShk?.Shake(_objShakeBallPreset);  
  }
  public void Shake(bool move)
  {
    if(move)
      _objShk?.Shake(_objShakeBallMovePreset);
    else
      _objShk?.Shake(_objShakeBallPreset);  
  }
  public void ExplodeBomb()
  {
    onBombExplode?.Invoke(this);
  }
  public void Explode()
  {
    onExplode?.Invoke(this);
  }
  public void PreExplode()
  {
    _ocb.SetColor(color);
    Color c = _colorMatchPing;
    _ocb.BlendTo(c);
  }
  public void Matched()
  {
    if(!IsMatching)
    {
      _ocb.SetColor(color);
      Color c = _colorMatchPing;
      _ocb.BlendTo(c);
    }
    IsMatching = true;
  }
  void Rotate(Transform _model, float dt)
  {
    Vector3 vdirp = new Vector3(_dir.y, 0, -_dir.x);
    float angle = (dt * 360 / _circum);
    _model.Rotate(vdirp, angle, Space.World);
  }
  void Update()
  {
    _lifetime += Time.deltaTime;
  }
  void OnDrawGizmos()
  {
    Gizmos.color = _color;
    Gizmos.DrawCube(transform.position + new Vector3(0,1.0f, 0), Vector3.one * 0.25f);
  }
}

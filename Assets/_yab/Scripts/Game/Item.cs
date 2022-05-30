using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.Utilities;

public class Item : MonoBehaviour
{
  [SerializeField] GameObject _arrow;
  [SerializeField] Color      _color;
  [SerializeField] ActivatableObject _activatable;
  [SerializeField] Spec       _special = Spec.None;

  public enum Spec
  {
    None,
    Static,
    Bomb,
    ColorChange
  }


  MeshRenderer _mr = null;
  MaterialPropertyBlock _mpb = null;

  Vector2Int _vturn = new Vector2Int(0,1);
  Vector2Int _dir = Vector2Int.zero;
  Vector2Int _gridBeg = Vector2Int.zero;
  Vector2Int _gridEnd = Vector2Int.zero;
  float _lifetime = 0;


  public static System.Action<Item> onShow, onHide; 

  static Vector2Int  toGridT(Vector3 vpos, Vector2Int vdir) => new Vector2Int((int)Mathf.Round(vpos.x - vdir.x * 0.5f), (int)Mathf.Round(vpos.z - vdir.y * 0.5f));  
  static Vector3     toPos(Vector2Int grid) => new Vector3(grid.x, 0,  grid.y);
  public static bool EqType(Item item0, Item item1)
  {
    return item0 != null && item1 != null && item0.name.Equals(item1.name);
  }

  void Awake()
  {
    _mr = GetComponentInChildren<MeshRenderer>();
    _mpb = new MaterialPropertyBlock();
    _mr.GetPropertyBlock(_mpb, 0);

    color = _color;
    grid = new Vector2Int(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.z));
    var v = transform.localRotation * new Vector3(0, 0, 1);
    _vturn.x = Mathf.RoundToInt(v.x);
    _vturn.y = Mathf.RoundToInt(v.z);

    _activatable.ActivateObject();
    onShow?.Invoke(this);
  }
  public Color color
  {
    get => _color; 
    set
    { 
      _color = value; 
      _mr.material.color = _color;
      _mpb.SetColor("_MainColor", _color);
      _mr.SetPropertyBlock(_mpb);
    }
  }
  public Vector2Int grid /*{get; set;}*/ = Vector2Int.zero;
  public Vector2Int gridNext => grid + _dir.to_units();
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
 
  Vector3 vdir => new Vector3(dir.x, 0, dir.y);
  public bool IsReady => !_activatable.InTransition && _lifetime > 0.125f;
  public bool IsMoving => grid != _gridEnd;
  public bool IsArrowVis {get => _arrow.activeSelf; set{_arrow.SetActive(value || IsStatic);}}
  public bool IsStatic => _special == Spec.Static;
  public bool IsBomb => _special == Spec.Static;
  public bool IsColorChanger => _special == Spec.ColorChange;

  public void Hide()
  {
    onHide?.Invoke(this);
    _activatable.DeactivateObject();
    StartCoroutine(WaitForEnd());
    //gameObject.SetActive(false);
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

  public bool MoveP(float dt)
  {
    if(!IsReady)
      return false;
    transform.localPosition += vdir * dt;
    var grid_prev = grid;
    grid = toGridT(transform.localPosition, dir);
    
    return grid != grid_prev;
  }
  public void PushBy(Vector2Int pushDir)
  {
    if(IsStatic)
      return;
      
    _dir = pushDir;
    _gridEnd = grid + _dir;
  }
  public void PushTo(Vector2Int gridDest)
  {
    if(IsStatic)
      return;

    _dir = (gridDest - grid).to_units();
    _gridEnd = gridDest;
  }
  public void Stop()
  {
    _dir = Vector2Int.zero;
    _gridEnd = grid;
    transform.localPosition = toPos(grid);
  }
  public bool Move(float dt, Vector2Int _dim)
  {
    if(!IsReady)
      return false;

    bool ret = true;
    transform.localPosition = Vector3.MoveTowards(transform.localPosition, toPos(_gridEnd), dt);
    if(Mathf.Approximately(Vector3.Distance(transform.localPosition, toPos(_gridEnd)), 0))
    {
      _dir = Vector2Int.zero;
      grid = _gridEnd;
      transform.localPosition = toPos(_gridEnd);
      ret = false;
    }
   
    return ret;
  }
  public void Change(Item itemFrom)
  {
    color = itemFrom.color;
    name = itemFrom.gameObject.name;
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

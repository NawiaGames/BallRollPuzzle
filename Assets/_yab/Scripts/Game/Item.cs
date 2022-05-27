using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
  [SerializeField] GameObject _arrow;
  [SerializeField] Color      _color;
  

  MeshRenderer _mr = null;
  MaterialPropertyBlock _mpb = null;

  Vector2Int _vturn = new Vector2Int(0,1);
  Vector2Int _dir = Vector2Int.zero;
  Vector2Int _gridBeg = Vector2Int.zero;
  Vector2Int _gridEnd = Vector2Int.zero;

  //static Vector2Int toGridT(Vector3 vpos, Vector2Int vdir) => new Vector2Int((int)(vpos.x - vdir.x * 0.9999f), (int)(vpos.z - vdir.y * 0.9999f));
  static Vector2Int toGridT(Vector3 vpos, Vector2Int vdir) => new Vector2Int((int)Mathf.Round(vpos.x - vdir.x * 0.5f), (int)Mathf.Round(vpos.z - vdir.y * 0.5f));  
  //static Vector2Int toGridT(Vector3 vpos, Vector2Int vdir) => new Vector2Int((vpos.x - vdir.x * 0.9999f), (int)(vpos.z - vdir.y * 0.9999f));
  //static Vector2Int toGridT(Vector3 vpos) => new Vector2Int((int)System.Math.Round((float)vpos.x, System.MidpointRounding.AwayFromZero), (int)(int)System.Math.Round((float)vpos.y, System.MidpointRounding.AwayFromZero));
  static Vector3    toPos(Vector2Int grid) => new Vector3(grid.x, 0,  grid.y);

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
  public Vector2Int gridNext => grid + _dir;
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

  public bool IsMoving => grid != _gridEnd;
  public void ArrowVis(bool vis)
  {
    _arrow.SetActive(vis);
  }
  public bool MoveP(float dt)
  {
    transform.localPosition += vdir * dt;
    var grid_prev = grid;
    grid = toGridT(transform.localPosition, dir);
    
    return grid != grid_prev;
  }
  public void Push(Vector2Int pushDir)
  {
    _dir = pushDir;
    _gridEnd = grid + _dir;
  }
  public void Stay()
  {
    _dir = Vector2Int.zero;
    _gridEnd = grid;
    transform.localPosition = toPos(grid);
  }
  public bool Move(float dt)
  {
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

  void OnDrawGizmos()
  {
    Gizmos.color = _color;
    Gizmos.DrawCube(transform.position + new Vector3(0,1.0f, 0), Vector3.one * 0.25f);
  }
}

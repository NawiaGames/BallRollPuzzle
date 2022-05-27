using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
  [SerializeField] SpriteRenderer _sr;
  [SerializeField] Color _colorNormal;
  [SerializeField] Color _colorSelected;

  bool    _selected = false;
  Vector2Int _grid = Vector2Int.zero;

  void Awake()
  {
    _sr.color = _colorNormal;
  }
  public bool        IsSelected { get => _selected; set { _selected = value; _sr.color = (_selected) ? _colorSelected : _colorNormal; } }
  public Vector3     vPos => transform.localPosition;
  public Vector2Int  grid {get; set;}
  public Vector2Int  dir {get; set;}
  public Vector3     vDir => new Vector3(dir.x, 0, dir.y);
}

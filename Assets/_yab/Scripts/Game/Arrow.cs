using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.Utilities;

public class Arrow : MonoBehaviour
{
  [SerializeField] SpriteRenderer _sr;
  [SerializeField] Color _colorNormal;
  [SerializeField] Color _colorSelected;
  [SerializeField] Color _colorBlocked;
  Color targetColor = Color.white;
  [SerializeField] float blendSpeed = 4f;
  [SerializeField] ActivatableObject _actObj;

  bool    _selected = false;
  bool    _blocked = false;
  Vector2Int _grid = Vector2Int.zero;

  void Awake()
  {
    _sr.color = _colorNormal;
    targetColor = _colorNormal;
  }
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

  private void Update() {
    _sr.color = Color.Lerp(_sr.color, targetColor, Time.deltaTime * blendSpeed);
  }
}

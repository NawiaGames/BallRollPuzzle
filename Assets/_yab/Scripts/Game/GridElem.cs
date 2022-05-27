using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElem : MonoBehaviour
{
  [SerializeField] Renderer   rend;
  [SerializeField] GameObject _fx;
  [SerializeField] Color      colorEven;
  [SerializeField] Color      colorOdd;
  
  bool _even = true;

  public bool even{get => _even; set{_even = value; SetColor();}}

  void SetColor()
  {
    rend.material.color = (_even)? colorEven : colorOdd;
  }
}

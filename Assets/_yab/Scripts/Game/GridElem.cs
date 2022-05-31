using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElem : MonoBehaviour
{
  [SerializeField] Renderer   rend;
  [SerializeField] GameObject _fx;
  [SerializeField] Color      colorEven;
  [SerializeField] Color      colorOdd;
  [SerializeField] AnimationCurve animCurve;
  
  bool _even = true;

  public bool even{get => _even; set{_even = value; SetColor();}}

  void SetColor()
  {
    rend.material.color = (_even)? colorEven : colorOdd;
  }
  IEnumerator coTouch(float delay)
  {
    yield return new WaitForSeconds(delay);
    Vector3 v = Vector3.zero;
    float t = 0;
    while(t < 1)
    {
      v.y = -animCurve.Evaluate(Mathf.Clamp01(t)) * 0.2f;
      _fx.transform.localPosition = v;
      t += Time.deltaTime * 4;
      yield return null;
    }
    _fx.transform.localPosition = Vector3.zero;
  }
  public void Touch(float delay)
  {
    //StartCoroutine(coTouch(delay));
  }
}
